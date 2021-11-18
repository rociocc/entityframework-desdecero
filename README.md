# Entity Framework desde cero

<ol>
  <li>
    Paquetes a instalar en el proyecto donde está la clase Context:
    - Npgsql.EntityFrameworkCore.PostgreSQL
    - Microsoft.EntityFrameworkCore.Design
    - Microsoft.EntityFrameworkCore.Tools
  </li>
  <li>
    En el proyecto API, instalar el paquete Microsoft.EntityFrameworkCore con la misma versión que los paquetes Microsoft.EntityFrameworkCore instalados en el paso anterior.
  </li>
  <li>
    Añadir la configuracion en el archivo Startup.cs, método ConfigureServices:
    
    services.AddDbContext<EFTestContext>(
      options =>  
      options.UseNpgsql(Configuration.GetConnectionString("ConnectionStringName"))
    );  
   
  </li>
  <li>
    Add the connection string section to the appsettings.json file:
    
    "ConnectionStrings": {
      "ConnectionStringName": "User ID =postgres;Password=password;Server=localhost;Port=5432;Database=MyTestDB;Integrated Security=true;Pooling=true;"
    }
    
  </li>
  <li>
    Crear los modelos del tipo:
    
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        
        // from the group model (Entity framework will connect the    	   
        // Primarykey and forign key)
        public Group Group { get; set; }
        public int GroupId { get; set; }
    }
    
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
  </li>
  <li>
    Crear la clase DBContext con cada una de las entidades:
    
    public class EFTestContext : DbContext
    {
        public EFTestContext(DbContextOptions<EFTestContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
    }
    
  </li>
</ol>

## Qué requiere EF para correr los comandos de Migración satisfactoriamente?
Algunos de los comandos de EF Tools (por ejemplo los usados en migraciones), requieren crear una instancia de DbContext en tiempo de diseño para poder obtener todos los detalles de las Entidades y cómo se mapean en un esquema de base de datos.
Los Tools usan diferentes maneras para crear la instancia de DbContext:   
<ol>
  <li>
    A partir de los servicios de la aplicación:
    
    // Extraido de https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli

    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        // EF Core uses this method at design time to access the DbContext
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder => webBuilder.UseStartup<Startup>());
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
            => services.AddDbContext<ApplicationDbContext>();

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {}
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
    }
   
  </li>
  <li>
    Usando un constructor sin parámetros:
    
Si los Tools no pueden obtener el DbContext del proveedor de servicios de la aplicación, buscan el tipo que derive de DbContext dentro del proyecto, e intentan crear la instancia usando el constructor sin parámetros.
    
  </li>
  <li>
    A partir del design-time factory proporcionado por EF:
    
Si se implementa la interfaz Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<TContext> se le puede decir a los Tools como crear la instancia de DbContext.
    
EF busca primero que todo, dentro del mismo proyecto de la clase derivada de DbContext o dentro del proyecto de inicio, si existe una clase que implemente esta interfaz, las Tools ignoran las anteriores maneras de crear una instancia y usan el design-time factory.
    
    public abstract class DesignTimeDbContextFactoryBase<TContext> :
        IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        private const string ConnectionStringName = "EFLabDatabaseCS";
        private const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";

        public TContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory() + string.Format("{0}..{0}EFLab", Path.DirectorySeparatorChar);
            return Create(basePath, Environment.GetEnvironmentVariable(AspNetCoreEnvironment));
        }

        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

        private TContext Create(string basePath, string environmentName)
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Local.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(ConnectionStringName);

            return Create(connectionString);
        }

        private TContext Create(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string '{ConnectionStringName}' is null or empty.", nameof(connectionString));
            }

            Console.WriteLine($"DesignTimeDbContextFactoryBase.Create(string): Connection string: '{connectionString}'.");

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return CreateNewInstance(optionsBuilder.Options);
        }
    }
    
Esta manera de crear el DbContext es útil cuando se separa la capa de acceso a datos (Persistence) en un proyecto diferente al que contiene la configuración de los servicios de la aplicación, por Inyección de Dependencias.
   
Al crear la instancia de DbContext, se requiere acceso al connectionString guardado en los settings, y si la capa de acceso a datos está en un proyecto diferente al del API, se le tiene que decir donde los puede encontrar.
  </li>
</ol>
    
Si se está usando VS for MAC, funciona mejor al usar desde la terminal el siguiente comando para crear la carpeta de Migraciones con la creación de las tablas asociadas a las entidades creadas hasta el momento, dandole el nombre al archivo de InitialMigration.
    
Ubicarse en la terminal en la carpeta del proyecto donde se ubica el DbContext.
    
    dotnet ef migrations add InitialMigration
 
Luego se actualiza la base de datos y se crean las tablas realmente con el siguiente comando:
    
    dotnet ef database update
    
