Left ToDo:
Implement MS Identity so it can be used for authentication and authorization in the application. This includes setting up user registration, login and refreshtoken. It should use jwttoken and refreshtoken.
Implement Employee class with properties for Name, Age, and Position. 
Implement Workload class with properties for CustomerId, EmployeeId, Start, Stop and Comments.
Make sure User will be used in interceptor as replacement for EmployeeId in Workload class. User should be used in the interceptor to set the EmployeeId in Workload class, but also as value for CreatedBy, UpdatedBy and DeletedBy in the three entities Customer, Employee and Workload.
Maybe switch to dapper for read operations for better performance and simplicity (Separation of Concerns).
Create a set of skills for this kind of pattern, covering minimal api with vertical slice according to this template.