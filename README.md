# Authentication and Authorization in ASP.NET Core MVC Using Cookies

# Core Concepts and Code Breakdown

- Cookie Authentication:
CookieAuthenticationDefaults.AuthenticationScheme is used to specify that cookie-based authentication will be used.
After a successful login, the user's details (like UserName) are stored in a cookie.
Claims-Based Identity:

- Claims are key-value pairs representing user information. Each user has a ClaimsIdentity that holds these claims.
A ClaimsPrincipal represents the current user, which is created from the ClaimsIdentity.

- CRUD Operations on External Data: Integrates with an external API that stores user data in JSON format.(checkout here https://retoolapi.dev/tggm1I/data)
