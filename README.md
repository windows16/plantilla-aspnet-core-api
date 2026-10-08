# Configuración local y secretos

Los valores sensibles de Supabase no deben almacenarse en `appsettings.json`,
ni enviarse al repositorio. `WebApplication.CreateBuilder` carga User Secrets
automáticamente en el entorno `Development` y las variables de entorno en
cualquier entorno.

## User Secrets (desarrollo local)

Desde la carpeta del proyecto:

```powershell
dotnet user-secrets set "Supabase:Url" "https://<referencia-proyecto>.supabase.co"
dotnet user-secrets set "Supabase:Issuer" "https://<referencia-proyecto>.supabase.co/auth/v1"
dotnet user-secrets set "Supabase:ServiceRoleKey" "<clave-service-role>"
```

Para revisar los nombres configurados sin mostrar sus valores:

```powershell
dotnet user-secrets list
```

## Variables de entorno

ASP.NET Core convierte `__` en `:` al cargar variables anidadas. En Windows:

```powershell
$env:Supabase__Url = "https://<referencia-proyecto>.supabase.co"
$env:Supabase__Issuer = "https://<referencia-proyecto>.supabase.co/auth/v1"
$env:Supabase__ServiceRoleKey = "<clave-service-role>"
```

En producción, se recomienda inyectar estas variables desde el administrador de
secretos de la plataforma de despliegue, en lugar de definirlas manualmente.
