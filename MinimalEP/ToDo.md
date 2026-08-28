Fundera kring följande förbättringar och kom med ideer/tankar samt en plan/förslag för implementation:
Kanske arbeta bort Employee och konsolidera denna entitet till AspNetUsers.
Införa roles i MsIdentity så man kan särskilja mellan olika typer av användare (t.ex. Admin, Employee, Customer) utan att behöva en separat Employee-entitet.
Utöka scenario med att admin ska kunna redigera andra användares information, t.ex. ändra roller, resetta lösenord, etc.
Admin bör även kunna lista användares arbetade timmar och annan relevant information.
Tänk typ ett enkelt tidsrapporteringssystem...Admin skapar kunder och anställda, anställda loggar timmar mot kunder, admin kan se rapporter. Typ så, kom med input om olika scenarios som du ser...
I dessa olika scenarios vill jag att du tänker utifrån kända patterns & practices, hur dessa kan färga denna kod och visa på vikten av att använda en välskriven kod med hjälp av abstraktioner, OOP, SOLID, Clean Code, Clean Architecture och GoF design patterns. Ett skolboksexempel helt enkelt, som visar på varför vi bör använda dessa metoder när vi utvecklar system.
