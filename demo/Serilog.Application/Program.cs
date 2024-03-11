var logger = DIContainer.Instance.Logger;
var message = new UserCredentials
{
    UserName = "MyUser",
    Password = "MySecretPassword"
};
logger.Information("Using the default will not leak the password for {message}", message);
logger.Information("Using explicit destructuring will not leak the password for {@message}", message);
logger.Information("Using explicit stringification WILL leak the password for {$message}", message);
