# Описание проекта "Telegram Remote Control - TRC" 

![Preview](Images/trc_img1.png)

## Языки
- [Українська](./README_UA.md)
- [English](./README.md)
- [Русский](./README_RU.md)

**"Telegram remote control - TRC "** - это проект приложения, предназначенного для удаленного управления некоторыми функциями компьютера через бота в мессенджере Telegram, разработанный с использованием **WPF .NET Core**. К основным функциям относятся:
- Изменение яркости монитора компьютера;
- Изменение общей громкости системы;
- Создание скриншота в режиме реального времени;
- Вызов "Пользовательской команды", другими словами, открытие приложений или файлов по команде пользователя;
- Среди прочего есть **Белый список**, для пользователей Telegram, которые могут подключаться и управлять компьютером.


## Как пользоваться приложением:
После открытия приложения в левом верхнем углу вы увидите 3 вкладки
- Команды,
- Настройки,
- Информация.

Если вы перейдете в раздел ***"Информация "***, то увидите инструкции о том, как создать Telegram-бота и использовать его для взаимодействия с приложением.
Ключевым элементом является **API-ключ**, который указывается в разделе настроек в поле ***"API-ключ от BotFather "***.

![Предварительный просмотр](Images/trc_img4_.png)

Также в настройках необходимо указать путь для сохранения скриншотов, по умолчанию это будет **системная папка Pictures текущего пользователя системы**.

Не забывайте про **White list**, для добавления туда нового пользователя необходимо нажать на кнопку с крестиком в правом нижнем углу списка, и ввести ник пользователя Telegram, который идет через **'@'**, сам этот символ вводить не нужно. 

После добавления пользователь с таким ником сможет управлять целевым компьютером через бота, API-ключ которого указывается при запуске в настройках.
Чтобы активировать бота, необходимо нажать на кнопку ***"Включить "*** в разделе ***"Настройки "***.

## "Пользовательские команды":
![Preview](Images/trc_img2.png)

Механизм использования таких команд позволяет запустить приложение или открыть файл на компьютере по его пути. 
Чтобы создать новый триггер для команды:

 Перейдите в раздел ***"Команды "*** -> **нажмите на кнопку с крестиком в правом нижнем углу** -> **в появившейся панели выберите путь к файлу и введите команду, по которой будет срабатывать триггер**. 
Для выполнения отправьте эту команду боту, который синхронизирован с вашим компьютером через приложение.

## Панель инструментов быстрого доступа:
Панель открывается в диалоге с самим ботом, на ней расположены 5 кнопок:
- Управление яркостью - ***"Яркость "***
- Регулировка громкости - ***"Громкость "***
- Сделать скриншот - ***"Скриншот "***
- Список триггеров - ***"Пользовательские команды "***
- Инструкции - ***"Информация "***

![Предварительный просмотр](Images/trc_img3.png)

## Регуляторы яркости и громкости:
Чтобы управлять яркостью или громкостью, необходимо выбрать соответствующую функцию на **"Панели быстрого доступа "**.
В диалоге с ботом появится панель с кнопками, где можно настроить уровни, а также узнать текущий уровень яркости/громкости.

## Скриншот:
Чтобы сделать скриншот на **"Панели быстрого доступа "**, нужно выбрать ***"ScreenShot "***, сделанный скриншот будет отправлен в диалог с ботом, а также сохранен по пути, установленному в ***"Настройки "*** -> ***"Путь для сохранения снимков "***.

## Заключение:
Подводя итог всему вышесказанному, можно сказать, что данный пет-проект является стартовой реализацией проекта по удаленному управлению компьютером с интуитивно понятным графическим интерфейсом. Целью данного зоопроекта была демонстрация его в колледже.
