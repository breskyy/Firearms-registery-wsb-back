# EWeaponRegistry - Cyfrowa Rejestracja i Obsługa Broni Palnej

Projekt studencki REST API dla systemu cyfrowej rejestracji i obsługi broni palnej na rynku cywilnym.

## Opis projektu

System centralnego rejestru broni palnej, który:
- Zastępuje papierowe książeczki broni
- Umożliwia cyfrową rejestrację i podgląd jednostek broni
- Obsługuje składanie wniosków o e-promesy
- Weryfikuje uprawnienia nabywców
- Rejestruje sprzedaż i transfery broni
- Prowadzi pełny audit log operacji

## Technologie

- **.NET 8** - ASP.NET Core Web API
- **PostgreSQL 16** - baza danych
- **Entity Framework Core 8** - ORM
- **JWT** - autentykacja
- **Docker & Docker Compose** - konteneryzacja
- **Swagger/OpenAPI** - dokumentacja API

## Wymagania

- Docker Desktop
- .NET 8 SDK (opcjonalnie, do development)

## Szybki start

### Uruchomienie przez Docker (zalecane)

```bash
docker compose up --build
```

Po uruchomieniu:
- **Swagger UI**: http://localhost:5000/
- **Health check**: http://localhost:5000/api/v1/health

### Uruchomienie lokalne (development)

```bash
# Uruchom PostgreSQL
docker compose up postgres -d

# Uruchom migracje
dotnet ef database update --project src/EWeaponRegistry.Infrastructure --startup-project src/EWeaponRegistry.Api

# Uruchom API
cd src/EWeaponRegistry.Api
dotnet run
```

## Dane logowania testowych użytkowników

| Email | Hasło | Rola |
|-------|-------|------|
| admin@example.com | Admin123! | Admin |
| officer@example.com | Officer123! | WpaOfficer |
| citizen@example.com | Citizen123! | Citizen |
| shop@example.com | Shop123! | Shop |

## Role użytkowników

### Citizen (Obywatel)
- Podgląd własnych danych, pozwoleń i broni
- Składanie wniosków o e-promesy
- Zgłaszanie zbycia broni
- Akceptacja/odrzucenie transferów

### Shop (Sklep koncesjonowany)
- Weryfikacja uprawnień nabywcy
- Weryfikacja promesy po QR kodzie
- Rejestracja sprzedaży broni

### WpaOfficer (Pracownik WPA)
- Przeglądanie obywateli i broni
- Obsługa wniosków o promesy
- Przeszukiwanie rejestru
- Podgląd alertów medycznych

### Admin
- Zarządzanie użytkownikami i rolami
- Przeglądanie audit logów
- Dostęp do słowników systemowych
- Endpointy mockowych integracji

## Najważniejsze endpointy

### Autentykacja
- `POST /api/v1/auth/login` - logowanie
- `GET /api/v1/auth/me` - dane zalogowanego użytkownika

### Obywatel
- `GET /api/v1/citizen/me` - profil
- `GET /api/v1/citizen/me/permits` - pozwolenia
- `GET /api/v1/citizen/me/firearms` - broń
- `POST /api/v1/citizen/me/promise-applications` - wniosek o promesę
- `POST /api/v1/citizen/me/transfer-requests` - zbycie broni

### Sklep
- `POST /api/v1/shop/verify-permit` - weryfikacja uprawnień
- `POST /api/v1/shop/firearms/register-sale` - rejestracja sprzedaży

### WPA
- `GET /api/v1/wpa/citizens` - lista obywateli
- `GET /api/v1/wpa/firearms` - wyszukiwanie broni
- `GET /api/v1/wpa/promise-applications` - wnioski o promesy
- `POST /api/v1/wpa/promise-applications/{id}/approve` - zatwierdzenie wniosku
- `GET /api/v1/wpa/medical-alerts` - alerty medyczne

### Admin
- `GET /api/v1/admin/users` - lista użytkowników
- `GET /api/v1/admin/audit-logs` - audit log
- `GET /api/v1/admin/dictionaries` - słowniki systemowe

## Bezpieczeństwo

### Szyfrowanie danych wrażliwych
Dane osobowe (PESEL, imię, nazwisko, adres) są szyfrowane algorytmem **AES-256-CBC**. Klucz szyfrowania jest pobierany z konfiguracji i **nigdy nie powinien być hardkodowany**.

### Audit Log
Wszystkie krytyczne operacje są zapisywane w audit log:
- Logowanie (udane i nieudane)
- Podgląd danych osobowych
- Rejestracja broni
- Transfer właścicielski
- Zatwierdzenie/odrzucenie wniosków
- Naruszenia reguł biznesowych

### Autentykacja JWT
- Token Bearer z rolą użytkownika
- Czas wygaśnięcia: 60 min (konfigurowalny)
- Autoryzacja przez `[Authorize(Roles = "...")]`

### HTTPS/TLS
**Produkcyjnie wymagane HTTPS/TLS 1.3!**
Lokalnie Docker działa po HTTP dla uproszczenia. W środowisku produkcyjnym skonfiguruj reverse proxy (nginx/traefik) z certyfikatem SSL.

## Potencjalne integracje zewnętrzne

**UWAGA:** Obecna wersja projektu zawiera wyłącznie **MOCKI** dla poniższych systemów. Nie są wykonywane żadne prawdziwe połączenia zewnętrzne.

### Zaimplementowane interfejsy (bez prawdziwej integracji):
- **mObywatel** - generowanie QR kodów dla promes
- **login.gov.pl / Węzeł Krajowy** - weryfikacja tożsamości
- **Operator płatności** - potwierdzenie opłat
- **Rejestr WPA** - weryfikacja numerów legitymacji
- **Push notifications** - powiadomienia

### Wymagania do prawdziwej integracji:
- Dostęp do środowisk testowych
- Certyfikaty i klucze API
- Dokumentacja techniczna dostawców
- Formalne zatwierdzenia
- OAuth2/OpenID Connect
- Komunikacja HTTPS/TLS
- Przechowywanie sekretów w bezpiecznym vault

### Endpointy mockowych integracji (tylko Admin, tylko Development):
- `POST /api/v1/integration/mock/national-login/verify`
- `POST /api/v1/integration/mock/mobywatel/generate-qr`
- `POST /api/v1/integration/mock/payments/confirm`
- `POST /api/v1/integration/mock/wpa/verify-weapon-book`
- `POST /api/v1/integration/mock/push/send`

## Reguły biznesowe

1. **Ważność pozwolenia** - musi być aktywne i nieprzeterminowane
2. **Limit broni** - nie można przekroczyć `maxFirearms` w pozwoleniu
3. **Ważność badań** - medyczne i psychologiczne muszą być aktualne
4. **Unikalność numeru seryjnego** - nie można zarejestrować duplikatu
5. **Zgodność kategorii** - broń musi pasować do typu pozwolenia
6. **Ważność promesy** - aktywna, nieprzeterminowana, z pozostałą ilością
7. **Atomowość sprzedaży** - wszystko lub nic (transakcja bazodanowa)
8. **Atomowość transferu** - sprawdzenie uprawnień nabywcy

## Struktura projektu

```
/src
  /EWeaponRegistry.Api          # Kontrolery, Middleware
  /EWeaponRegistry.Application  # DTOs, Interfejsy serwisów
  /EWeaponRegistry.Domain       # Encje, Enumy
  /EWeaponRegistry.Infrastructure # DbContext, Serwisy, Mocki
/tests
  /EWeaponRegistry.Tests        # Testy jednostkowe
docker-compose.yml
Dockerfile
CLAUDE_TASKS.md
README.md
```

## Konfiguracja

### Zmienne środowiskowe (docker-compose.yml / .env)
```
POSTGRES_PASSWORD=YourPassword
JWT_SECRET_KEY=32CharacterMinimumSecretKey!!!!!
ENCRYPTION_KEY=32ByteAES256EncryptionKey!!!!!
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "EWeaponRegistry",
    "Audience": "EWeaponRegistryUsers",
    "ExpirationMinutes": 60
  },
  "Encryption": {
    "Key": "..." // 32 znaki dla AES-256
  }
}
```

## Ograniczenia projektu studenckiego

1. **Brak prawdziwych integracji** - wszystkie zewnętrzne systemy są mockowane
2. **Dane testowe fikcyjne** - nie używamy prawdziwych danych osobowych
3. **HTTP lokalnie** - HTTPS wymagane produkcyjnie
4. **Brak frontendu** - tylko REST API
5. **Uproszczone reguły** - nie wszystkie przepisy prawa o broni
6. **Brak powiadomień** - push notifications tylko symulowane

## Testy

```bash
# Uruchom testy
dotnet test

# Testy z coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Licencja

Projekt studencki WSB - do celów edukacyjnych.

## Autor

Projekt Wdrożeniowy - WSB
