# Arkitekturell och teknisk genomlysning av MinimalEP

## Sammanfattning

MinimalEP har en tydlig och pedagogisk grund: .NET 10 Minimal API, vertikala slices, explicita handlers, FluentValidation, `Result<T>`, `TypedResults`, EF Core för skrivningar och Dapper för läsningar. Lösningen bygger utan fel.

De ursprungliga P0/P1-riskerna kring behörighetsgränser, datainvarianter, tokenrotation och atomära kontooperationer är åtgärdade. Fas 1–4 är genomförda och optimistic concurrency har därefter införts för redigerbara entiteter. Kvarvarande punkter är huvudsakligen behovs- eller policyberoende förbättringar, inte kända kritiska fel.

Repositoryprestanda har profilerats och benchmarkats före och efter Fas 3. Resultaten redovisas längre ned; produktionsnära lasttest med större datamängder och samtidiga klienter återstår.

## Genomförandestatus

Fas 1 är implementerad i kod:

- employee- och customer-hantering kräver `AdminOrAbove`; `/me` förblir self-service
- den fristående `AddEmployee`-slicen är borttagen; konto och Employee skapas tillsammans
- generell workload-update kan inte längre ändra `Stop`
- en filtrerad unik databasconstraint skyddar mot flera öppna workloads per employee
- publik registrering tilldelar endast `User`; explicit SuperAdmin-bootstrap är avstängd som standard och fungerar endast i en tom installation
- konto-, roll- och Employee-operationer använder transaktioner och kontrollerar Identity-resultat
- sista SuperAdmin och självdegradering skyddas
- refresh tokens använder tokenfamilj och rowversion för reuse- och samtidighetsskydd
- login använder Identity-lockout och auth-endpoints har rate limiting
- central exception handling med Problem Details är aktiverad

Migrationen `HardenPhaseOne` måste appliceras i respektive miljö. Fas 1:s centrala behörighets-, repository- och samtidighetsregler regressionsverifieras av testportföljen i Fas 2.

Fas 2 är implementerad:

- `MinimalEP.Tests` innehåller en HTTP-baserad authorization-matris för anonyma användare, User, Admin och SuperAdmin
- SQL Server-integrationstester verifierar repository soft delete, unik öppen workload och refresh-token-rowversion
- bootstrap- och JWT-inställningar använder typed options med startupvalidering
- databasens startupmigrering styrs av `DatabaseOptions`
- `/health/live` och `/health/ready` är tillgängliga utan autentisering; readiness kontrollerar SQL Server
- OpenTelemetry samlar HTTP-, HttpClient- och SQL-spårning samt HTTP-metrics och använder OTLP när `OTEL_EXPORTER_OTLP_ENDPOINT` är satt
- säkerhetshändelser loggas strukturerat utan råa tokens eller e-postadresser
- EF Core sensitive-data-logging är borttagen

Fas 3 och Fas 4 är implementerade. Dessutom är optimistic concurrency införd:

- `Customer`, `Employee` och `Workload` använder SQL Server `rowversion`
- read responses exponerar concurrency-token som Base64-kodat `byte[]`
- update, `/me` och workload stop kräver klientens senast lästa `RowVersion`
- stale writes returnerar `409 Conflict` och uppmanar klienten att läsa om resursen
- migrationen `AddEditableEntityConcurrency` måste appliceras i respektive miljö
- SQL Server-integrationstest verifierar att en stale customer-uppdatering avvisas
- solution build och samtliga 29 tester är gröna

## Omfattning och metod

Genomlysningen omfattar:

- projekt- och paketkonfiguration
- API-startup och endpointregistrering
- vertikala slices och kärnabstraktioner
- autentisering, auktorisering och tokenflöden
- EF Core-konfiguration, Dapper-repositories och soft delete
- validering, felhantering, observability och testbarhet
- statiska prestanda- och skalbarhetsrisker
- förekomst av magic strings och magic values

Verifierat i nuläget:

- hela lösningen bygger utan fel
- `MinimalEP.Tests` innehåller 29 gröna tester
- repositorybaseline och eftermätning har genomförts med BenchmarkDotNet och CPU-profilering
- produktionsnära belastningsmätning ingår ännu inte

## Styrkor och fördelar

### 1. Tydlig vertikal slice-struktur

Use cases är separerade i request, response, validator, mapping, handler och endpoint. Det ger hög lokal sammanhållning och gör förändringar enklare att isolera än i en traditionell horisontell controller/service-struktur.

### 2. Små, explicita handlers

Handlers har generellt ett tydligt ansvar och använder `IRequestHandler<TRequest, TResponse>`. Detta förenklar testning och gör applikationsflöden lätta att följa.

### 3. Konsekvent HTTP-mappning

Endpoints använder `TypedResults` och mappar `Result<T>` explicit till HTTP-resultat. Det minskar risken för oavsiktliga statuskoder och håller HTTP-lagret separat från applikationslogiken.

### 4. Central valideringspipeline

`ValidationFilter<TRequest>` undviker upprepad valideringskod i varje endpoint. FluentValidation-reglerna ligger nära respektive use case.

### 5. Bra grund för identitet och tokens

- ASP.NET Core Identity används i stället för egen lösenordshantering.
- Refresh tokens lagras hashade med SHA-256.
- Access tokens validerar issuer, audience, livslängd och signeringsnyckel.
- `MapInboundClaims = false` används konsekvent.
- Roller är redan centraliserade i `Roles`.

### 6. Säker ägarskapskontroll för flera workload-flöden

`GetWorkload`, `GetWorkloads`, `StopWorkload` och `DeleteWorkload` begränsar vanliga användare till egna poster och returnerar 404 vid otillåten åtkomst. Det minskar informationsläckage och BOLA/IDOR-risk.

### 7. Separata läs- och skrivstrategier

Dapper används för projekterade läsningar medan EF Core används för spårade skrivningar och interceptorbaserad audit/soft delete. Det är en pragmatisk CQRS-liknande uppdelning utan onödig infrastruktur.

### 8. Bra databasgrund

- UUID v7 ger bättre indexlokalitet än slumpmässiga UUID:n.
- SQL-frågor är parametriserade.
- `DbContext` poolas.
- Konfigurationer ligger i separata `IEntityTypeConfiguration<T>`.
- `Address` modelleras som owned type.
- Refresh-token-hash har unikt index.

## Preliminär prioritering

| Rank | Prioritet | Finding | Typ | Bedömning |
|---:|:---:|---|---|---|
| 1 | P0 | Generella employee-endpoints saknar roll-/resursauktorisering | Säkerhet/dataintegritet | Verifierat i kod |
| 2 | P0 | `UpdateWorkload` kan sätta eller nollställa `Stop` och kringgå punch-clock-invarianten | Domän/dataintegritet | Verifierat i kod |
| 3 | P0 | Första publika registreringen kan bli SuperAdmin; kontrollen är race-känslig | Säkerhet | Verifierat i kod |
| 4 | P1 | Identity-user, roller och Employee sparas inte atomärt | Dataintegritet | Verifierat i kod |
| 5 | P1 | Unik öppen workload garanteras inte av databasen | Samtidighet/dataintegritet | Verifierat i kod |
| 6 | P1 | Refresh-token-rotation saknar concurrency-skydd och fullständig reuse-hantering | Säkerhet/samtidighet | Verifierat i kod |
| 7 | P1 | Login/register/refresh saknar rate limiting och login använder inte lockoutflöde | Säkerhet | Verifierat i kod |
| 8 | P1 | Customer-flödet har inkonsekvent unik e-post + soft delete och update-kollisioner | Dataintegritet | Verifierat i kod |
| 9 | P1 | Generella customer-endpoints är öppna för alla autentiserade användare | Auktorisering | Verifierat; önskad policy måste beslutas |
| 10 | P1 | Rolländringar är flerstegsoperationer utan rollback och Identity-resultat ignoreras | Dataintegritet/säkerhet | Verifierat i kod |
| 11 | P1 | Ingen central exception handling/Problem Details och ingen strukturerad applikationsloggning | Driftbarhet | Verifierat i kod |
| 12 | P1 | Inga automatiserade tester finns i lösningen | Kvalitet | Verifierat i lösningen |
| 13 | P2 | List-endpoints saknar paginering och materialiserar hela resultatuppsättningen | Prestanda | Statisk skalbarhetsrisk |
| 14 | P2 | Dapper-anrop förmedlar inte `CancellationToken` | Resurshantering | Verifierat i kod |
| 15 | P2 | `StartWorkload` hämtar alla användarens workloads för en existenskontroll | Prestanda | Statisk skalbarhetsrisk |
| 16 | P2 | Dapper-joins filtrerar inte soft-deletade Customer/Employee | Korrekthet | Verifierat i kod |
| 17 | P2 | JWT/configuration använder strängnycklar, null-forgiving och runtime-parse utan startupvalidering | Konfiguration | Verifierat i kod |
| 18 | P2 | Databasmigrering körs automatiskt vid varje applikationsstart | Drift/deployment | Designrisk |
| 19 | P2 | Endpoint/validator-kopplingen bygger på namespace och upprepad reflection | Underhåll/starttid | Verifierat; prestandapåverkan ej mätt |
| 20 | P2 | Databasmodell och API saknar optimistic concurrency för redigerbara entiteter | Samtidighet | Åtgärdat med `rowversion`, API-token och 409-hantering |
| 21 | P2 | Valideringsgränser och databaskolumnlängder är duplicerade och delvis inkonsekventa | Korrekthet/magic values | Verifierat i kod |
| 22 | P3 | `Created`-headers pekar inte på de faktiska versionerade API-routes | API-kvalitet | Verifierat i kod |
| 23 | P3 | `SELECT *` och SQL-/kolumnnamn är hårdkodade | Underhåll/magic strings | Verifierat i kod |
| 24 | P3 | Tid hämtas direkt från statiska klockor | Testbarhet/magic values | Verifierat i kod |
| 25 | P3 | Kommentarer, språk och formattering är inkonsekventa | Underhåll | Verifierat i kod |

## Detaljerade findings

### F-01 — Employee-endpoints saknar tillräcklig auktorisering (P0)

`Program.cs:59-62` kräver endast att användaren är autentiserad. `GetEmployeesEndpoint.cs:11`, `AddEmployeeEndpoint.cs:11`, `UpdateEmployeeEndpoint.cs:11` och `DeleteEmployeeEndpoint.cs:11` lägger inte på någon administrativ policy och handlers gör ingen resurskontroll.

Konsekvenser:

- en vanlig användare kan lista alla anställda och deras profiluppgifter
- en vanlig användare kan ändra eller soft-deleta andra anställda
- `AddEmployee` kan skapa en `Employee` utan motsvarande `ApplicationUser`, trots systemets uttalade invariant `ApplicationUser.Id == Employee.Id`

Rekommendation:

- begränsa administrativa employee-routes med en central policykonstant, exempelvis `AuthorizationPolicies.AdminOrAbove`
- behåll `/me` som separat self-service-slice
- avveckla eller omdefiniera `AddEmployee`; konto + employee bör skapas via ett enda atomärt provisioningflöde
- lägg integrationstester för User/Admin/SuperAdmin per route

### F-02 — `UpdateWorkload` kringgår punch-clock-modellen (P0)

`UpdateWorkloadRequest.cs:3` exponerar både `Start` och `Stop`, och `UpdateWorkloadMapping.cs:11-13` skriver båda. En vanlig användare kan därför återöppna en stängd workload genom att skicka `Stop = null`, eller stänga den utan `StopWorkload`.

Det motsäger domänregeln att `StopWorkload` är enda slicen som får sätta `Stop`, och kan även kringgå kontrollen om högst en öppen workload.

Rekommendation:

- ta bort `Stop` från `UpdateWorkloadRequest`
- låt generell update endast ändra tillåtna metadata, exempelvis kommentarer
- definiera separat use case om starttid ska kunna korrigeras, sannolikt endast för administratör
- lägg domäntester som visar att en stängd workload inte kan återöppnas via generell update

### F-03 — Bootstrap av SuperAdmin är publik och race-känslig (P0)

`RegisterEndpoint` är anonymt. `RegisterHandler.cs:41-44` räknar användare efter skapandet och gör den användare som ser count 1 till SuperAdmin/Admin/User.

Risker:

- första externa klienten kan ta SuperAdmin-rollen i en tom installation
- parallella första registreringar gör bootstrap-resultatet timingberoende
- `Count()` är synkront mot databasen

Rekommendation:

- flytta bootstrap till deployment/seeding med hemlighet eller explicit engångskommando
- stäng publik självregistrering om produktkravet är att admin skapar anställda
- om självregistrering ska finnas: tilldela alltid lägsta roll och separera bootstrap helt

### F-04 — Konto, roller och employee-post sparas inte atomärt (P1)

`RegisterHandler.cs:31-66` och `CreateEmployeeAccountHandler.cs:39-67` utför flera Identity- och EF-operationer utan en gemensam transaktion. Om rolltilldelning eller employee-save misslyckas kan en orphaned Identity-user lämnas kvar. Om Identity lyckas men Employee misslyckas kan användaren inte logga in korrekt.

Dessutom kontrolleras inte resultatet från `AddToRolesAsync`/`AddToRoleAsync`.

Rekommendation:

- inför en applikationstjänst/unit of work som använder samma `ApplicationDbContext`-transaktion för hela operationen
- kontrollera varje `IdentityResult`
- rollbacka eller kompensera deterministiskt vid fel
- fånga unikhetskonflikter och mappa dem till stabila domän-/HTTP-resultat

### F-05 — Unik öppen workload saknar databasskydd (P1)

`StartWorkloadHandler.cs:18-25` gör check-then-insert. Två samtidiga requests kan båda se att ingen öppen post finns och därefter skapa varsin.

Rekommendation:

- skapa ett filtrerat unikt index för `EmployeeId` där `Stop IS NULL AND Deleted IS NULL`
- ersätt hämtning av alla poster med `HasOpenWorkloadAsync`/`EXISTS`
- mappa unique-constraint-felet till 409 Conflict

### F-06 — Refresh-token-rotation är race-känslig (P1)

`RefreshTokenHandler.cs:67-95` läser, verifierar, revokerar och skapar ersättning utan concurrency token eller villkorad update. Två samtidiga requests med samma token kan båda hinna rotera den.

Kommentaren om reuse detection motsvarar dessutom endast att den presenterade token nekas; ingen tokenfamilj eller alla efterföljande tokens revokeras vid misstänkt återanvändning.

Rekommendation:

- lägg optimistic concurrency/rowversion eller atomisk villkorad update
- modellera tokenfamilj/session och revokera familjen vid återanvändning
- logga säkerhetshändelsen utan att logga rå token
- överväg ett index för aktiva tokens per användare/session och en cleanup-strategi

### F-07 — Brute-force-skydd saknas (P1)

`LoginHandler.cs:22-24` använder `CheckPasswordAsync`, vilket inte driver Identity-lockout. Ingen rate limiter registreras för login, register eller refresh.

Rekommendation:

- använd `SignInManager.CheckPasswordSignInAsync(..., lockoutOnFailure: true)` eller motsvarande explicit lockoutflöde
- lägg separata rate-limit policies för auth-endpoints
- returnera samma externa felrespons för okänd användare och fel lösenord
- logga aggregerade misslyckanden utan PII/hemligheter

### F-08 — Customer-e-post och soft delete är inkonsekventa (P1)

`CustomerConfiguration.cs:28` skapar ett ovillkorligt unikt index på e-post. `CustomerRepository.cs:45` kontrollerar däremot endast aktiva kunder. Efter soft delete godkänner applikationskontrollen återanvändning, men databasen avslår insert.

`UpdateCustomerHandler` kontrollerar inte e-postunikhet alls. Detta kan ge obehandlade databasfel i stället för 409.

Rekommendation:

- besluta om e-post ska kunna återanvändas efter soft delete
- använd filtrerat unikt index om svaret är ja; kontrollera annars även soft-deletade poster
- normalisera e-post konsekvent
- hantera konkurrerande inserts/updates genom constraint + exception mapping, inte enbart pre-check

### F-09 — Customer-endpoints saknar administrativ policy (P1)

Alla autentiserade användare kan skapa, läsa, ändra och radera kunder eftersom endpointsen ärver enbart gruppens generella authorization. `ToDo.md` beskriver däremot scenariot att admin skapar kunder.

Rekommendation:

- fastställ en accessmatris
- lägg `AdminOrAbove` på skrivningar om detta är avsikten
- avgör om vanliga användare ska kunna läsa alla kunder, en begränsad projektion eller endast kunder kopplade till egna workloads

### F-10 — Rolländringar kan lämna inkonsekvent eller låst system (P1)

`AssignRoleHandler.cs:36-39` tar först bort alla roller och lägger sedan till en ny utan transaktion eller kontroll av `IdentityResult`. Ett fel i steg två lämnar användaren utan roll. En SuperAdmin kan även degradera sig själv eller den sista SuperAdmin-användaren.

Rekommendation:

- kontrollera alla Identity-resultat
- gör rollbytet atomärt
- skydda sista SuperAdmin och överväg förbud mot självdegradering
- logga rolländringar som säkerhetsaudit

### F-11 — Central felhantering och observability saknas (P1)

`Program.cs` registrerar ingen `AddProblemDetails`, `UseExceptionHandler`, health checks eller applikationsspecifik strukturerad loggning. Databas-/Identity-fel riskerar därför att bli generiska 500-svar utan stabil felmodell eller tillräcklig korrelationsdata.

Rekommendation:

- inför central exception mapping till RFC 9457 Problem Details
- lägg correlation/trace id i felrespons och logg
- lägg health/readiness checks för SQL Server
- instrumentera auth, databaslatens, requestlatens och fel med OpenTelemetry eller motsvarande
- undvik PII, JWT och refresh tokens i loggar

### F-12 — Automatiserade tester saknas (P1)

Lösningen innehåller inget testprojekt. De mest riskfyllda reglerna är samtidigt behörighet, samtidighet och flerstegstransaktioner, vilka är svåra att säkra med manuell testning.

Rekommenderad första testportfölj:

1. integrationstester för alla endpoint-policyer och ägarskapsregler
2. integrationstester för register/provisioning och rollback
3. samtidighetstest för start av workload
4. samtidighetstest för refresh-token-rotation
5. repositorytester för soft delete och unik e-post
6. enhetstester för validators och mappings
7. arkitekturtester för beroenderiktning och slice-konventioner

### F-13 — Listningar saknar paginering (P2, statisk skalbarhetsrisk)

`CustomerRepository.GetAllAsync` (`CustomerRepository.cs:33-38`), `EmployeeRepository.GetAllAsync` (`EmployeeRepository.cs:54-59`) och `WorkloadRepository.GetAllAsync` (`WorkloadRepository.cs:62-68`) läser och materialiserar hela tabeller.

Riskerna är ökande databas-I/O, minnesallokering, serialiseringstid och stora svar. Detta är sannolikt en framtida flaskhals men är inte profileringsverifierat.

Rekommendation:

- använd obligatorisk, maxbegränsad paginering
- föredra keyset/cursor pagination för stora och föränderliga tabeller
- definiera stabil sortering, exempelvis `(Created, Id)` eller `(Start, Id)`
- returnera tunna DTO-projektioner
- mät med realistiska radantal före och efter

### F-14 — Dapper-anrop ignorerar cancellation (P2)

Repository-metoder tar emot `CancellationToken`, men Dapper-anropen skickar inte token vidare. Avbrutna HTTP-requests kan därför fortsätta belasta SQL Server och anslutningspoolen.

Rekommendation:

- använd `CommandDefinition` med `cancellationToken`
- inför central command timeout via typed options
- lägg integrationstest där en avbruten request avbryter databasoperationen

### F-15 — Öppen-workload-kontrollen läser för mycket (P2, statisk skalbarhetsrisk)

`StartWorkloadHandler.cs:18-20` hämtar samtliga workloads för användaren och kör sedan `Any` i minnet. Kostnaden växer med hela historiken.

Rekommendation:

- inför `HasOpenWorkloadAsync(employeeId, ct)` med SQL `EXISTS`
- stöd frågan med det filtrerade unika indexet från F-05
- benchmarka eller profilera med realistisk historik

### F-16 — Dapper-joins respekterar inte soft delete för relaterade entiteter (P2)

`WorkloadRepository.BaseSelect` filtrerar `w.Deleted`, men inte `c.Deleted` eller `e.Deleted`. Workloads kan därför returneras med soft-deletad kund eller employee. Dapper omfattas inte av EF:s query filters.

Rekommendation:

- lägg explicita filter för alla soft-deletade tabeller i handskriven SQL
- definiera önskat historikbeteende: dölj posten, visa snapshot eller visa markerad borttagen relation
- testa att Dapper- och EF-vägar ger samma semantik

### F-17 — JWT-konfiguration är strängbaserad och valideras sent (P2)

`JwtTokenService.cs:17-36`, `AuthExtensions.cs:34-58`, `LoginHandler.cs:34` och `RefreshTokenHandler.cs:25-26,79` använder upprepade nyckelsträngar, null-forgiving och `int.Parse`/`double.Parse` vid runtime. Produktionskonfigurationen har dessutom tom nyckel i `appsettings.json`.

Rekommendation:

- skapa `JwtOptions` med konstanter för section name
- bind med Options pattern
- använd `ValidateDataAnnotations`, egen validator och `ValidateOnStart`
- representera durationer som validerade heltal eller `TimeSpan`
- kontrollera minsta nyckellängd och förbjud tomma issuer/audience
- injicera `IOptions<JwtOptions>` i tokenflödena

### F-18 — Automatisk migrering vid startup är en driftrisk (P2)

`Program.cs:36-37` migrerar och seeder innan appen startar. Det är bekvämt lokalt, men i produktion kan flera repliker konkurrera, startup blockeras och applikationsidentiteten behöver DDL-rättigheter.

Rekommendation:

- kör migration som separat deployment-jobb i produktion
- behåll automatisk migrering endast bakom explicit development/demo-konfiguration
- logga och verifiera resultat från role seeding; `CreateAsync`-resultatet ignoreras idag

### F-19 — Endpointregistreringen är konventionskänslig (P2)

`EndpointExtensions.cs:58-89` skannar alla typer igen för varje endpoint och kopplar handler till endpoint genom att de råkar ligga i samma namespace. Detta är skört vid flera handlers/endpoints i samma namespace och ger onödig startup-reflection.

Rekommendation:

- gör requesttypen explicit i endpointkontrakt/metadata
- bygg registreringsmetadata en gång vid startup eller använd source generation
- skapa startup-/arkitekturtest som verifierar exakt en avsedd handler per endpoint

Detta är främst en underhållsrisk. Startupkostnaden måste mätas innan den prioriteras som prestandafel.

### F-20 — Optimistic concurrency saknas (P2)

Customer, Employee och Workload har ingen rowversion/concurrency token. Samtidiga uppdateringar blir last-write-wins och kan tyst skriva över varandra.

Rekommendation:

- lägg `rowversion` på redigerbara entiteter
- exponera ETag eller versionsfält i API:t
- kräv `If-Match` eller motsvarande versionskontroll vid update/delete
- returnera 409/412 vid konflikt

### F-21 — Validering och databasgränser är duplicerade (P2)

Exempel:

- åldersgränsen `16..100` upprepas i flera validators
- namn, adress, telefon och position har databaslängder men validators saknar motsvarande `MaximumLength`
- Customer email har 255 tecken medan Employee email har 256
- workload comments har databasgräns 1000 men bör valideras före save

Konsekvensen kan bli databasfel i stället för 400 och successiv drift mellan slices.

Rekommendation:

- skapa domännära constraints, exempelvis `EmployeeConstraints`
- återanvänd constraints i validators och EF-konfiguration
- undvik att centralisera affärsregler som saknar gemensam betydelse; centralisera bara verkligt delade invariants

### F-22 — Felaktiga eller ofullständiga `Location`-headers (P3)

Created-svar använder exempelvis `/customers/{id}` och `/workloads/{id}`, medan faktisk route ligger under `/api/v{version:apiVersion}`. Register pekar dessutom på `/auth/{userId}`, där ingen motsvarande GET-route syns.

Rekommendation:

- namnge GET-endpoints och använd `CreatedAtRoute`
- inkludera aktuell API-version
- använd endast Location till en resurs som faktiskt kan hämtas

### F-23 — Hårdkodad SQL och `SELECT *` (P3)

`CustomerRepository.cs:29,37,45` innehåller SQL och tabell-/kolumnnamn direkt i metoderna. `SELECT *` gör mapping känslig för schemaförändringar och hämtar mer data än API:t behöver.

Rekommendation:

- välj explicita kolumner och projektera till read models
- håll queries nära slicen eller i tydligt namngivna queryobjekt
- undvik ett generiskt repository-lager som döljer frågans avsikt
- lägg repository-integrationstester mot riktig SQL Server

### F-24 — Direkt användning av systemklocka (P3)

`DateTimeOffset.UtcNow` och `DateTime.UtcNow` används i token- och auditlogik. Det försvårar deterministiska tester av expiry och audit.

Rekommendation:

- injicera .NET `TimeProvider`
- använd samma tidskälla för token expiry, refresh-tokenaktivitet och audit

### F-25 — Kodstil och kommentarer är inkonsekventa (P3)

Kodbasen blandar svenska och engelska kommentarer, två- och fyrstegsindrag samt långa pedagogiska kommentarer med triviala numreringar. Det skadar inte runtime men gör mallen mindre konsekvent.

Rekommendation:

- lägg `.editorconfig`
- välj ett språk för kodkommentarer och felmeddelanden
- behåll kommentarer som förklarar varför, ta bort sådant som endast beskriver vad koden gör
- aktivera analyzers och behandla relevanta warnings som errors

## Magic strings och magic values

### Identifierade kategorier

| Kategori | Exempel | Rekommenderad åtgärd |
|---|---|---|
| Configuration paths | `"Jwt"`, `"Jwt:RefreshTokenExpiresInDays"`, `"DefaultConnection"` | Typed options och `SectionName`/connection-name-konstanter |
| Authorization policies | `"AdminOrAbove"`, `"SuperAdminOnly"` | `AuthorizationPolicies`-klass |
| Routes | `"/employees"`, `"/workloads/{id}"`, `"api/v{version:apiVersion}"` | Routekonstanter eller namngivna routes; undvik övercentralisering av unika routes |
| JWT claims | `"name"`, `"age"`, `"position"` | Claim-konstanter eller standardclaims där semantiken passar |
| Token durations | `60`, `7` | `JwtOptions` med validerade egenskaper |
| Password policy | längd `8`, temporärt lösenordsformat | `IdentityOptions` som single source of truth; generatorn bör läsa policyn eller vara separat tjänst |
| Validation limits | ålder `16..100`, comments `1000`, stränglängder | Delade domänconstraints |
| SQL identifiers | tabell-/kolumnnamn, `splitOn: "Street"`, `splitOn: "Id,Id"` | Explicita read models, SQL-konstanter lokalt och integrationstester |
| Felmeddelanden | upprepade auth-/conflict-texter | Stabil felkod + lokaliserbart meddelande; undvik en global textsäck |
| OpenAPI/UI | `"MinimalEP API"`, theme/client | Typed dokumentationskonfiguration om miljöberoende |
| Tid | `DateTimeOffset.UtcNow`, `DateTime.UtcNow` | `TimeProvider` |

### Föreslagen struktur

- `Infrastructure/Auth/JwtOptions.cs`
- `Infrastructure/Auth/AuthorizationPolicies.cs`
- `Domain/Model/EmployeeConstraints.cs`
- `Domain/Model/CustomerConstraints.cs`
- `Domain/Model/WorkloadConstraints.cs`
- namngivna routes per aggregate eller use case
- stabila felkoder i `Result<T>`/Problem Details

Allt ska inte bli en global konstant. En sträng som bara används en gång och är lokal för ett use case kan vara tydligare där den är. Centralisering bör göras när värdet är en delad invariant, ett externt kontrakt eller en konfigurationsnyckel.

## Prestanda och flaskhalsar

### Statiskt identifierade risker

1. Obegränsade listqueries materialiserar hela tabeller.
2. Dapper ignorerar requestens cancellation token.
3. `StartWorkload` läser hela användarens historik för en booleanfråga.
4. Workload-listning gör multi-mapping av fulla Customer/Employee-objekt i stället för en tunn read model.
5. Indexen stödjer foreign keys men inte tydligt de vanligaste filtren tillsammans med soft delete och sortering.
6. Registreringsflödet använder synkront `Count()`.
7. Endpointregistrering gör upprepad reflection vid startup.
8. Refresh tokens saknar beskriven retention/cleanup och kan växa obegränsat.

### Vad som bör mätas

Skapa först realistiska datamängder, exempelvis:

- 100 000 customers
- 25 000 employees
- 5–20 miljoner workloads
- flera års workloads per employee
- flera aktiva och historiska refresh tokens per användare

Mät sedan:

| Scenario | Primära mått |
|---|---|
| `GET /workloads` med och utan filter | p50/p95/p99, SQL-duration, reads, allokeringar, response size |
| `GET /employees` och `GET /customers` | latens och minne relativt radantal |
| `POST /workloads/start` | latens, rows read, samtidighetsfel |
| login/refresh | latens, DB roundtrips, contention vid parallell refresh |
| application startup | migrationstid och endpointregistrering |

Rekommenderad ordning:

1. lägg integrationstest/benchmark som reproducerar respektive query
2. samla CPU- och allocation-trace
3. fånga SQL execution plans och logical reads
4. ändra en sak i taget
5. kör samma mätning efter ändringen och dokumentera före/efter

### Fas 3 — uppmätt resultat

En BenchmarkDotNet-baseline kördes mot LocalDB med 1 000 customers, 1 000 employees och 10 000 workloads. Samma benchmarkartifact kördes före och efter ändringen.

| Metod | Före | Efter | Förändring |
|---|---:|---:|---:|
| `GetAllCustomers` | 526,8 µs | 141,7 µs | −73,1 % |
| `GetAllEmployees` | 810,1 µs | 232,9 µs | −71,3 % |
| `GetAllWorkloads` | 36 249,3 µs | 372,3 µs | −99,0 % |
| `GetEmployeeWorkloadHistory` | 33 226,2 µs | 402,7 µs | −98,8 % |

CPU-andelen i `SqlDataReader.GetValue` sjönk från 52,78 % till 9,25 %. Efterprofilen visar ingen kvarvarande betydande kostnad i egen repositorykod; `WorkloadRepository.QueryPageAsync` stod för 0,04 % total CPU.

Genomfört:

- keyset-pagination med UUID v7-cursor, standardsida 50 och max 100
- `nextCursor` i listresponser
- `CommandDefinition` och request-cancellation i samtliga Dapper-anrop
- SQL `EXISTS` för kontroll av öppen workload
- filtrering av soft-deletade Customer/Employee i workload-joins
- filtrerade sammansatta index för `(EmployeeId, Id)` och `(CustomerId, Id)`
- integrationstester för sidgräns, cursor utan överlapp och cancellation

Tunna read models infördes inte i denna iteration: efter paginering är den uppmätta repositorykostnaden 0,14–0,40 ms och profilen pekar på SqlClient/Dapper, inte en handlingsbar hotspot i egen kod. Refresh-token-retention lämnas också till en separat mätcykel eftersom faktisk volym, revisionskrav och retentionstid först måste fastställas; att radera säkerhetsdata utan sådan policy är inte en säker prestandaoptimering.

## Arkitekturell bedömning

### Det som bör behållas

- vertikala slices
- explicita request/response-kontrakt
- separata mappings och validators
- `Result<T>` som applikationsresultat
- Dapper för optimerade read models och EF Core för transaktionella writes
- central endpointdiscovery, men med robustare metadata
- `/me` som separat resursorienterad self-service-yta

### Det som bör förstärkas

#### Domäninvarianter

Nu ligger flera regler endast i handlers. Kritiska invariants bör ha flera skyddslager:

1. requestvalidering för snabb feedback
2. domänmetod för korrekt tillståndsövergång
3. databasconstraint/index för samtidighet

`Workload.Start`, `Workload.Stop` och öppet/stängt tillstånd är den tydligaste kandidaten. Överväg metoder som `Start`, `Stop` och `CorrectComments` i stället för publika setters överallt.

#### Transaktionsgränser

Repository per aggregate fungerar för enkla operationer, men konto-provisioning korsar Identity och Employee. Där behövs en explicit transaktionsgräns. Undvik att skapa ett allmänt `GenericRepository<T>`; modellera i stället use-case-specifika transaktioner.

#### Read models

Dapper-frågor bör returnera slice-specifika DTO:er direkt. Att först materialisera rika domänobjekt och navigationer för listvyer skapar koppling och onödigt dataarbete.

#### Felmodell

`Result<T>` behöver sannolikt kompletteras med åtminstone validation, unauthorized/forbidden och ett stabilt domänfel med kod. Auth-fel bör inte modelleras som 404/409 enbart för att typerna saknas.

## Föreslagen åtgärdsplan

### Fas 1 — Stoppa säkerhets- och integritetsrisker

1. Lås employee- och customer-routes enligt en beslutad accessmatris.
2. Ta bort `Stop` från generell workload-update.
3. Flytta SuperAdmin-bootstrap från publik registrering.
4. Gör konto/employee/roll-provisioning atomär och kontrollera alla Identity-resultat.
5. Lägg databasconstraint för en öppen workload per employee.
6. Säkra refresh-token-rotation mot samtidighet.
7. Inför rate limiting, lockout och central Problem Details.

### Fas 2 — Skapa säkerhetsnät

1. Lägg testprojekt och authorization-matris.
2. Lägg SQL Server-integrationstester för repositories och constraints.
3. Lägg concurrency-tester.
4. Inför typed options med startupvalidering.
5. Lägg health checks, strukturerad loggning och tracing.

### Fas 3 — Skalbarhet

1. Klart: etablera profileringsbaseline och före-/eftermätning.
2. Klart: inför keyset-pagination; tunna read models är mätmässigt uppskjutna.
3. Klart: förmedla cancellation till Dapper.
4. Klart: ersätt historikläsning med `EXISTS`.
5. Klart: lägg index för faktisk filter- och cursorform.
6. Uppskjutet: definiera och mät refresh-token-retention i en separat cykel.

### Fas 4 — Underhållbarhet och magic values

1. Klart: centralisera policies, option sections och verkligt delade constraints.
2. Klart: synkronisera validatorer med databasgränser.
3. Klart: namnge routes och rätta `Location`-headers.
4. Klart: lägg `.editorconfig` och analyzers.
5. Klart: förenkla kommentarer och standardisera språk/formattering.

### Efter Fas 4 — Optimistic concurrency

1. Klart: lägg SQL Server `rowversion` på `Customer`, `Employee` och `Workload`.
2. Klart: inkludera `RowVersion` i läskontrakt och kräv det vid update, `/me` och workload stop.
3. Klart: mappa `DbUpdateConcurrencyException` till `409 Conflict`.
4. Klart: lägg migration och integrationstest för stale writes.

### Kvarstående och uppskjutet

1. Stärk `Workload` med domänmetoder för tillståndsövergångar.
2. Utöka `Result<T>` med stabila felkoder och explicita validation/unauthorized/forbidden-resultat.
3. Definiera revisions- och retentionpolicy för refresh tokens innan schemalagd rensning implementeras.
4. Inför `TimeProvider` för deterministisk testning av token-, audit- och tidslogik.
5. Ersätt namespace/reflection-kopplingen i endpointregistreringen endast om mätning eller underhållsproblem motiverar det.
6. Inför tunna read models endast om ny profilering visar att nuvarande mapping blivit en hotspot.
7. Kör produktionsnära lasttest med större datamängder, samtidiga klienter, p95/p99 och SQL execution plans.
8. Komplettera befintlig OpenTelemetry-instrumentering med en tracing-backend, exempelvis Jaeger via OTLP. Säkerställ att varje trace har ett unikt `TraceId` som följer W3C Trace Context genom inkommande HTTP-anrop, interna spans, `HttpClient` och SQL-anrop samt inkluderas i strukturerade loggar och Problem Details för korrelation.

## Definition of done för högprioriterade fixes

En P0/P1-finding bör inte markeras klar förrän:

- beteendet täcks av automatiserat test
- authorization testas både positivt och negativt
- databasconstraint finns för samtidighetskänslig invariant
- fel mappas till dokumenterat HTTP-svar
- loggning innehåller korrelations-id men inga hemligheter
- build och relevanta tester är gröna
- en prestandaändring har före-/eftermätning med samma workload

## Slutsats

Kodbasen är en bra pedagogisk start med tydliga slices och moderna .NET-mönster. Den viktigaste förflyttningen är från en fungerande demo till ett robust fleranvändarsystem: explicit accessmatris, databassäkrade invariants, atomära identitetsflöden, central drifttelemetri och automatiserade tester. Magic strings/values bör reduceras selektivt genom typed options, policykonstanter och delade domänconstraints. Prestandaarbetet bör börja med paginerings- och cancellation-riskerna, men faktiska flaskhalsar ska prioriteras först efter mätning.
