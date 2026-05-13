# EWeaponRegistry - Cyfrowa Rejestracja i Obsługa Broni Palnej

Projekt studencki REST API dla systemu cyfrowej rejestracji i obsługi broni palnej na rynku cywilnym.

## Opis projektu

System centralnego rejestru broni palnej, który:
- Obsługuje pełny cykl uzyskania pozwolenia na broń (wniosek → WPA → pozwolenie)
- Zastępuje papierowe książeczki broni
- Umożliwia cyfrową rejestrację i podgląd jednostek broni
- Obsługuje składanie wniosków o e-promesy
- Weryfikuje uprawnienia nabywców (pozwolenie, promesa, badania lekarskie)
- Rejestruje sprzedaż i transfery broni między obywatelami
- Umożliwia zarządzanie pozwoleniami przez WPA (zawieszenie, cofnięcie)
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
- Składanie wniosków o pozwolenie na broń (z datami badań lekarskich)
- Podgląd własnych danych, pozwoleń i broni
- Składanie wniosków o e-promesy
- Podgląd aktywnych promes z QR tokenem
- Zgłaszanie zbycia broni i akceptacja/odrzucenie transferów
- Zgłoszenie utraty/kradzieży broni
- Podgląd alertów medycznych

### Shop (Sklep koncesjonowany)
- Weryfikacja uprawnień nabywcy (pozwolenie, promesa, badania)
- Weryfikacja promesy po QR kodzie lub numerze promesy
- Atomowa rejestracja sprzedaży broni

### WpaOfficer (Pracownik WPA)
- Obsługa wniosków o pozwolenie na broń (approve/reject/require-correction)
- Obsługa wniosków o promesy
- Zarządzanie pozwoleniami (zawieszenie, cofnięcie, przywrócenie)
- Aktualizacja dat badań lekarskich na pozwoleniu
- Przeglądanie obywateli i wyszukiwanie broni
- Podgląd alertów medycznych

### Admin
- Zarządzanie użytkownikami i rolami
- Przeglądanie audit logów
- Dostęp do słowników systemowych
- Endpointy mockowych integracji

## Endpointy API

### Autentykacja
- `POST /api/v1/auth/login` - logowanie, zwraca JWT token
- `GET /api/v1/auth/me` - dane zalogowanego użytkownika

### Citizen — pozwolenia na broń
- `GET  /api/v1/citizen/me/permit-applications` - lista złożonych wniosków o pozwolenie
- `POST /api/v1/citizen/me/permit-applications` - złóż wniosek o pozwolenie (z datami badań)
- `GET  /api/v1/citizen/me/permits` - lista aktywnych pozwoleń

### Citizen — e-promesy
- `GET  /api/v1/citizen/me/promise-applications` - lista wniosków o promesę
- `POST /api/v1/citizen/me/promise-applications` - złóż wniosek o promesę
- `GET  /api/v1/citizen/me/promises` - aktywne promesy z QR tokenem

### Citizen — broń i transfery
- `GET  /api/v1/citizen/me/firearms` - lista broni
- `GET  /api/v1/citizen/me/firearms/{id}` - szczegóły + historia właścicieli
- `POST /api/v1/citizen/me/firearms/{id}/report-lost` - zgłoś utratę/kradzież
- `GET  /api/v1/citizen/me/transfer-requests` - lista transferów
- `POST /api/v1/citizen/me/transfer-requests` - zainicjuj transfer
- `POST /api/v1/citizen/me/transfer-requests/{id}/accept` - akceptuj transfer
- `POST /api/v1/citizen/me/transfer-requests/{id}/reject` - odrzuć transfer

### Citizen — inne
- `GET  /api/v1/citizen/me` - profil (PESEL maskowany)
- `GET  /api/v1/citizen/me/medical-alerts` - alerty o wygasających badaniach

### Sklep
- `POST /api/v1/shop/verify-permit` - weryfikacja promesy (QR lub numer)
- `POST /api/v1/shop/firearms/register-sale` - atomowa rejestracja sprzedaży

### WPA — wnioski o pozwolenie
- `GET  /api/v1/wpa/permit-applications` - lista wniosków (filtr: status)
- `GET  /api/v1/wpa/permit-applications/{id}` - szczegóły wniosku
- `POST /api/v1/wpa/permit-applications/{id}/mark-under-review`
- `POST /api/v1/wpa/permit-applications/{id}/approve` - zatwierdź (tworzy pozwolenie)
- `POST /api/v1/wpa/permit-applications/{id}/reject`
- `POST /api/v1/wpa/permit-applications/{id}/require-correction`

### WPA — wnioski o promesę
- `GET  /api/v1/wpa/promise-applications` - lista wniosków (filtr: status)
- `GET  /api/v1/wpa/promise-applications/{id}` - szczegóły wniosku
- `POST /api/v1/wpa/promise-applications/{id}/mark-under-review`
- `POST /api/v1/wpa/promise-applications/{id}/approve` - zatwierdź (tworzy promesę)
- `POST /api/v1/wpa/promise-applications/{id}/reject`
- `POST /api/v1/wpa/promise-applications/{id}/require-correction`

### WPA — zarządzanie pozwoleniami
- `POST  /api/v1/wpa/permits/{id}/suspend` - zawieś pozwolenie
- `POST  /api/v1/wpa/permits/{id}/revoke` - cofnij pozwolenie
- `POST  /api/v1/wpa/permits/{id}/restore` - przywróć zawieszone
- `PATCH /api/v1/wpa/permits/{id}/medical-exams` - aktualizuj daty badań

### WPA — przegląd i wyszukiwanie
- `GET /api/v1/wpa/citizens` - lista obywateli (paginacja)
- `GET /api/v1/wpa/citizens/{id}` - szczegóły obywatela
- `GET /api/v1/wpa/firearms` - wyszukiwarka broni (serialNumber, pesel, permitNumber, permitType)
- `GET /api/v1/wpa/medical-alerts` - alerty medyczne wszystkich obywateli

### Admin
- `GET  /api/v1/admin/users` - lista użytkowników
- `POST /api/v1/admin/users` - utwórz użytkownika
- `PATCH /api/v1/admin/users/{id}/role` - zmień rolę
- `PATCH /api/v1/admin/users/{id}/status` - aktywuj/dezaktywuj
- `GET  /api/v1/admin/audit-logs` - audit log
- `GET  /api/v1/admin/dictionaries` - słowniki systemowe (enumy)

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
3. **Ważność badań** - medyczne i psychologiczne muszą być aktualne przy zakupie i transferze
4. **Unikalność numeru seryjnego** - nie można zarejestrować duplikatu
5. **Zgodność kategorii** - broń musi pasować do typu pozwolenia (Sport: A/B, Ochrona: B, Łowiectwo: C)
6. **Ważność promesy** - aktywna, nieprzeterminowana, z pozostałą ilością
7. **Atomowość sprzedaży** - walidacja + tworzenie broni + aktualizacja promesy/pozwolenia w jednej transakcji
8. **Atomowość transferu** - weryfikacja uprawnień nabywcy przed zmianą właściciela
9. **Zgłoszenie utraty** - zwalnia slot w pozwoleniu, status broni: Lost
10. **Zawieszenie pozwolenia** - blokuje zakup nowej broni i składanie wniosków o promesę

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
