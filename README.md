# IFF


1. Загальний опис
"IFF" — це додаток, розроблений на платформі .NET MAUI для управління нарахуваннями зарплат, перегляду виплат та генерації звітів для працівників різних ролей (стрілець, фінансист, адміністратор). Програма дозволяє користувачам входити в систему, переглядати свої нарахування, фінансистам — нараховувати зарплати іншим працівникам, а також створювати PDF-звіти.
Версія програми: 1.0

Група: ФІТ 3-1 з
Розробник: Хаблак Данило Михайлович
________________________________________
2. Призначення
Програма призначена для:
•	Стрільців: Перегляд особистих нарахувань, подача скарг адміністратору.
•	Фінансистів: Нарахування зарплат працівникам, перегляд власних нарахувань, генерація звітів.
•	Адміністраторів: Управління користувачами (реєстрація, редагування, блокування), перегляд скарг, контроль нарахувань.
•	Забезпечення прозорості фінансових операцій та зручного доступу до даних.


3. Структура програми
3.1. Модель даних (IFF.Models)
User: Модель користувача:
•	Id (int): Унікальний ідентифікатор.
•	Username (string): Логін.
•	Password (string): Пароль.
•	FullName (string): Повне ім'я.
•	Role (string): Роль ("Стрілець", "Фінансист", "Адміністратор").
•	HireDate (DateTime?): Дата найму.
•	HasAccess (bool): Статус доступу.
Salary: Модель зарплати:
•	UserId (int): ID користувача.
•	Year (int): Рік.
•	Month (int): Місяць (1–12).
•	AccrualDate (DateTime): Дата нарахування.
•	Accrued, Pdfo, Pension, Military, ZfFund, Bonus, Received (decimal): Фінансові показники.
•	WorkDays, VacationDays, SickDays (int?): Кількість днів.
Complaint: Модель скарги:
•	UserId (int): ID користувача.
•	Username (string): Логін.
•	Text (string): Текст скарги.
•	Date (DateTime): Дата подання.
•	IsRoleChangeRequest (bool): Запит на зміну ролі.
3.2. Сервіси (IFF.Services)
DatabaseService: Клас для роботи з SQLite:
•	InitializeAsync: Ініціалізація бази.
•	GetUsersAsync, GetSalariesByUserAndYearAsync, SaveSalaryAsync, SaveComplaintAsync, DeleteUserAsync.
3.3. Перегляди (IFF.Views)
•	LoginPage: Сторінка входу.
•	RegisterPage: Сторінка реєстрації (для адміністратора).
•	WorkerDashboardPage: Дашборд стрільця.
•	FinancierDashboardPage: Дашборд фінансіста.
•	AdminDashboardPage: Дашборд адміністратора.
•	EditSalaryPage: Сторінка редагування зарплати.
3.4. Моделі перегляду (IFF.ViewModels)
•	LoginViewModel: Логіка входу.
•	WorkerDashboardViewModel: Логіка для стрільця.
•	FinancierDashboardViewModel: Логіка для фінансіста.
•	AdminDashboardViewModel: Логіка для адміністратора.
•	EditSalaryViewModel: Логіка редагування зарплати.
3.5. Головний клас (App)
•	Зберігає CurrentUser як статичну властивість.
•	Ініціалізує додаток із NavigationPage(new LoginPage()).


4. Функціональність
4.1. Вхід у систему
•	Сторінка: LoginPage.
•	Логіка: LoginViewModel.
•	Користувач вводить Username і Password.
•	Перевірка через DatabaseService.GetUserAsync.
•	Перехід до відповідного дашборду: 
o	WorkerDashboardPage для "Стрілець".
o	FinancierDashboardPage для "Фінансист".
o	AdminDashboardPage для "Адміністратор".
4.2. Роль: Стрілець (WorkerDashboardPage)
Опис
Інтерфейс для стрільця призначений для перегляду особистих нарахувань та подання скарг адміністратору.
Вкладки
Перегляд нарахувань:
•	Функції: 
o	Список зарплат за обраний рік (12 місяців).
o	Поля: Місяць, Дата, Нараховано, ПДФО, ПЗ, ВЗ, ФЗФ, Премія, Отримано.
o	Вибір року через Picker (AvailableYears).
o	Відображення FullName поточного користувача.
•	Команди: 
o	GenerateReportCommand: Генерація PDF-звіту з особистими нарахуваннями.
o	LogoutCommand: Вихід із системи.
Зв'язок:
•	Функції: 
o	Введення тексту скарги в Editor.
o	Надсилання скарги адміністратору.
•	Команди: 
o	SubmitComplaintCommand: Збереження скарги в базі.
o	LogoutCommand: Вихід.
Логіка (WorkerDashboardViewModel)
•	Конструктор: 
o	Ініціалізує _currentUser, Salaries, AvailableYears (роки від HireDate до поточного + 1).
•	Методи: 
o	InitializeAsync: Завантажує зарплати через LoadSalariesAsync.
o	LoadSalariesAsync: Завантажує 12 унікальних місяців із DatabaseService.
o	GenerateReportAsync: Створює PDF-звіт із Salaries.
o	SubmitComplaintAsync: Зберігає скаргу.
o	LogoutAsync: Очищає App.CurrentUser і повертається до LoginPage.

4.3. Роль: Фінансист (FinancierDashboardPage)
Опис
Інтерфейс для фінансіста дозволяє нараховувати зарплати іншим працівникам, переглядати власні нарахування та подавати скарги.
Вкладки
Нарахування:
•	Функції: 
o	Вибір працівника через Picker (Users).
o	Вибір року через Picker (AvailableYears).
o	Список зарплат обраного працівника (12 місяців).
o	Кнопка "Нарахувати" для редагування зарплати.
o	Відображення "Всього отримано" (TotalReceived).
•	Команди: 
o	EditSalaryCommand: Відкриває EditSalaryPage.
o	GenerateReportCommand: Генерація звіту.
o	LogoutCommand: Вихід.
Перегляд нарахувань:
•	Функції: 
o	Список особистих нарахувань (12 місяців).
o	Вибір року через Picker.
o	Відображення FullName.
•	Команди: 
o	GenerateReportCommand.
o	LogoutCommand.
Зв'язок:
•	Функції: 
o	Надсилання скарг адміністратору.
•	Команди: 
o	SubmitComplaintCommand.
o	LogoutCommand.
Логіка (FinancierDashboardViewModel)
•	Конструктор: 
o	Ініціалізує _currentUser, Users, SalariesForSelectedUser, SalariesForCurrentUser, AvailableYears.
•	Методи: 
o	InitializeAsync: Завантажує користувачів і зарплати.
o	LoadUsersAsync: Завантажує список працівників.
o	LoadSalariesAsync: Завантажує зарплати для SelectedUser.
o	LoadSalaryReportAsync: Завантажує особисті зарплати.
o	EditSalaryAsync: Перехід до редагування.
o	GenerateReportAsync: Генерація PDF.
o	SubmitComplaintAsync: Надсилання скарги.


4.4. Роль: Адміністратор (AdminDashboardPage)
Опис
Інтерфейс для адміністратора дозволяє управляти користувачами, переглядати скарги та власні нарахування.
Вкладки
Користувачі:
•	Функції: 
o	Список усіх користувачів (Users).
o	Редагування ролі, статусу доступу (HasAccess), видалення або додавання користувачів.
•	Команди: 
o	EditUserCommand: Зміна даних користувача.
o	DeleteUserCommand: Видалення користувача.
o	AddUserCommand: Перехід до RegisterPage.
o	LogoutCommand: Вихід.
Скарги:
•	Функції: 
o	Список скарг (Complaints).
o	Поля: Ім'я користувача, Текст, Дата, Тип (скарга/запит на роль).
o	Позначення скарги як вирішену.
•	Команди: 
o	ResolveComplaintCommand: Оновлення статусу скарги.
o	LogoutCommand: Вихід.
Перегляд нарахувань:
•	Функції: 
o	Список особистих нарахувань (12 місяців).
o	Вибір року через Picker.
•	Команди: 
o	GenerateReportCommand.
o	LogoutCommand.
Логіка (AdminDashboardViewModel)
•	Конструктор: 
o	Ініціалізує _currentUser, Users, Complaints, Salaries, AvailableYears.
•	Методи: 
o	InitializeAsync: Завантажує користувачів, скарги, зарплати.
o	LoadUsersAsync: Оновлює список користувачів.
o	LoadComplaintsAsync: Завантажує скарги.
o	LoadSalariesAsync: Завантажує особисті зарплати.
o	EditUserAsync: Зміна даних користувача.
o	DeleteUserAsync: Видалення користувача.
o	AddUserAsync: Реєстрація нового користувача.
o	ResolveComplaintAsync: Оновлення статусу скарги.

4.5. Редагування зарплати (EditSalaryPage)
•	Доступ: Лише для фінансіста.
•	Функції: 
o	Введення: BaseSalary, WorkDays, VacationDays, SickDays.
o	Автоматичний розрахунок податків і премії.
•	Команди: 
o	SaveCommand: Збереження в базі.
o	ClearCommand: Очищення полів.
o	CancelCommand: Повернення назад.


5. Вимоги до системи
•	Платформа: .NET MAUI (Android, iOS, Windows).
•	База даних: SQLite.
•	Залежності: Microsoft.Maui.Controls, PdfSharpCore, CommunityToolkit.Maui.Storage.



6. Інструкція з використання
6.1. Встановлення
•	Скомпілюйте проєкт у Visual Studio 2022 із .NET MAUI.
•	Встановіть на пристрій або запустіть у симуляторі.
6.2. Вхід
•	Введіть логін і пароль: 
o	worker / worker (Стрілець).
o	financer / financer (Фінансист).
o	admin / admin (Адміністратор).
•	Натисніть "Увійти".
6.3. Стрілець
•	Перегляньте нарахування, вибравши рік.
•	Сформуйте звіт через "Сформувати звіт".
•	Надішліть скаргу на вкладці "Зв'язок".
•	Вийдіть через "Вийти".
6.4. Фінансист
•	Нарахуйте зарплату: виберіть працівника, рік, натисніть "Нарахувати".
•	Перегляньте свої нарахування на вкладці "Перегляд".
•	Сформуйте звіт або надішліть скаргу.
•	Вийдіть.
6.5. Адміністратор
•	Керуйте користувачами: редагуйте, видаляйте, додавайте.
•	Перегляньте та вирішуйте скарги.
•	Перегляньте свої нарахування.
•	Вийдіть.





7. Технічні деталі
7.1. Розрахунок зарплати
•	Формула: 
o	Щоденна ставка = BaseSalary / Днів у місяці
o	Нараховано = (Щоденна ставка * WorkDays) + (Щоденна ставка * 0.75 * VacationDays) + (Щоденна ставка * 0.5 * SickDays)
o	ПДФО = Нараховано * -0.05
o	ПЗ = Нараховано * -0.07
o	ВЗ = Нараховано * -0.03
o	ФЗФ = Нараховано * -0.03
o	Премія = Нараховано * 0.25
o	Отримано = Нараховано + ПДФО + ПЗ + ВЗ + ФЗФ + Премія
7.2. База даних
•	Таблиці: Users, Salaries, Complaints.


8. Обмеження
•	Немає валідації введення.
•	Локальна база без синхронізації.


9. Рекомендації
•	Додати серверну синхронізацію.
•	Реалізувати фільтри та графіки.


10. Висновок
Програма "IFF" забезпечує базову функціональність для управління зарплатами та перегляду нарахувань. Вона підходить для невеликих організацій із чітко визначеними ролями. Оновлення коду усунули проблему дублювання місяців, і додаток готовий до використання з урахуванням зазначених інструкцій.

