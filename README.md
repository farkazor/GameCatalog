# Game Catalog
Настільний додаток на C# Windows Forms для ведення каталогу відеоігор з можливістю фільтрації, перегляду галереї обложек та медіа-вмісту.

# Реалізовані фічі
- Відображення списку ігор у DataGridView з підсвічуванням років.
- Фільтрація та пошук за категоріями/жанрами.
- Окреме вікно галереї з автоматичним слайд-шоу обложок.
- Інтерактивне вікно медіа з вбудованим браузером WebView2 для HTML-описів та перегляду відео.
- База даних SQLite для збереження інформації.

# Як запустити проєкт
1. Клонуйте репозиторій.
2. Відкрийте `.sln` файл у Visual Studio.
3. Переконайтеся, что в папці `bin/Debug` або `bin/Debug/net8.0-windows` є папка `Assets` та файл `catalog.db`.
4. Натисніть *Start*.

### Схема бази даних

```mermaid
erDiagram
    CATEGORIES ||--o{ ITEMS : "contains"
    
    CATEGORIES {
        int category_id PK
        string category_name
    }
    
    ITEMS {
        int item_id PK
        string title
        int category_id FK
        int release_year
        string cover_image_path
        string media_path
        string html_desc_path
    }
