# APBD Ćwiczenie 8 - Hospital API

Aplikacja korzysta z bazy danych SQL Server oraz Entity Framework Core w podejściu Database First.

## Technologie

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Docker
- Swagger

## Uruchomienie bazy danych

```bash
docker start hospital-sql
```

Baza danych została utworzona na podstawie pliku `create.sql`.

## Scaffold bazy danych

```bash
dotnet ef dbcontext scaffold "Server=localhost,1433;Database=HospitalDb;User Id=sa;Password=Your_password123;Encrypt=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --context HospitalDbContext --context-dir Data --output-dir Models --no-onconfiguring --force
```

## Uruchomienie aplikacji


## Endpointy

### GET `/api/patients`

Zwraca listę wszystkich pacjentów wraz z przyjęciami i przypisaniami do łóżek.

### GET `/api/patients?search=an`

Wyszukuje pacjentów po imieniu lub nazwisku.

### POST `/api/patients/{pesel}/bedassignments`

Przypisuje pacjentowi wolne łóżko danego typu na wybranym oddziale.

Przykładowe body:

```json
{
  "from": "2026-05-20T14:00:00",
  "to": "2026-05-30T10:00:00",
  "bedType": "Standard",
  "ward": "Kardiologia"
}
```

Pole `to` jest opcjonalne.
