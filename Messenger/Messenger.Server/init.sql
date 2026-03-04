-- =====================================================
-- Инициализация базы данных Messenger
-- =====================================================

-- Очистка существующих данных (если нужно пересоздать)
DELETE FROM messages;
DELETE FROM chat_participants;
DELETE FROM chats;
DELETE FROM user_status;
DELETE FROM users;
DELETE FROM departments;

-- Сброс счетчиков автоинкремента
DELETE FROM sqlite_sequence;

-- =====================================================
-- 1. Добавление отделов
-- =====================================================
INSERT INTO departments (name, description) VALUES 
('ИТ отдел', 'Информационные технологии и разработка'),
('Производство', 'Производственный цех и технологи'),
('HR отдел', 'Отдел кадров и рекрутинг'),
('Бухгалтерия', 'Финансовый отдел'),
('Отдел продаж', 'Продажи и работа с клиентами'),
('Логистика', 'Склад и доставка'),
('Администрация', 'Руководство предприятия'),
('Охрана труда', 'Безопасность и охрана труда');

-- =====================================================
-- 2. Добавление пользователей (пароль = username)
-- =====================================================
-- ИТ отдел (id = 1)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('ivanov', 'ivanov', 'Иванов Иван Иванович', 1, 'Разработчик'),
('petrov', 'petrov', 'Петров Петр Петрович', 1, 'Тимлид'),
('sidorov', 'sidorov', 'Сидоров Сидор Сидорович', 1, 'Тестировщик'),
('smirnov', 'smirnov', 'Смирнов Алексей Владимирович', 1, 'Системный администратор');

-- Производство (id = 2)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('kuznetsov', 'kuznetsov', 'Кузнецов Дмитрий Владимирович', 2, 'Начальник цеха'),
('popov', 'popov', 'Попов Андрей Сергеевич', 2, 'Инженер-технолог'),
('vasiliev', 'vasiliev', 'Васильев Николай Петрович', 2, 'Рабочий');

-- HR отдел (id = 3)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('smirnova', 'smirnova', 'Смирнова Анна Сергеевна', 3, 'HR-менеджер'),
('kozlov', 'kozlov', 'Козлов Денис Игоревич', 3, 'Специалист по подбору');

-- Бухгалтерия (id = 4)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('morozova', 'morozova', 'Морозова Елена Владимировна', 4, 'Главный бухгалтер'),
('novikova', 'novikova', 'Новикова Татьяна Александровна', 4, 'Бухгалтер');

-- Отдел продаж (id = 5)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('sokolov', 'sokolov', 'Соколов Максим Андреевич', 5, 'Руководитель отдела'),
('popova', 'popova', 'Попова Елена Андреевна', 5, 'Менеджер по продажам'),
('volkov', 'volkov', 'Волков Сергей Николаевич', 5, 'Менеджер по работе с клиентами');

-- Логистика (id = 6)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('vasiliev', 'vasiliev', 'Васильев Алексей Николаевич', 6, 'Логист'),
('grigoriev', 'grigoriev', 'Григорьев Иван Петрович', 6, 'Кладовщик');

-- Администрация (id = 7)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('director', 'director', 'Соколов Владимир Игоревич', 7, 'Генеральный директор'),
('secretary', 'secretary', 'Михайлова Ольга Дмитриевна', 7, 'Секретарь');

-- Охрана труда (id = 8)
INSERT INTO users (username, password, full_name, department_id, position) VALUES
('bezopasnost', 'bezopasnost', 'Федоров Игорь Викторович', 8, 'Инженер по охране труда');

-- =====================================================
-- 3. Создание внутренних чатов отделов
-- =====================================================
INSERT INTO chats (name, type, created_by) VALUES
('ИТ отдел', 'Department', 1),
('Производство', 'Department', 5),
('HR отдел', 'Department', 8),
('Бухгалтерия', 'Department', 10),
('Отдел продаж', 'Department', 12),
('Логистика', 'Department', 15),
('Администрация', 'Department', 17),
('Охрана труда', 'Department', 19);

-- =====================================================
-- 4. Добавление участников в чаты отделов
-- =====================================================
-- ИТ отдел (chat_id = 1)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(1, 1), (1, 2), (1, 3), (1, 4);

-- Производство (chat_id = 2)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(2, 5), (2, 6), (2, 7);

-- HR отдел (chat_id = 3)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(3, 8), (3, 9);

-- Бухгалтерия (chat_id = 4)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(4, 10), (4, 11);

-- Отдел продаж (chat_id = 5)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(5, 12), (5, 13), (5, 14);

-- Логистика (chat_id = 6)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(6, 15), (6, 16);

-- Администрация (chat_id = 7)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(7, 17), (7, 18);

-- Охрана труда (chat_id = 8)
INSERT INTO chat_participants (chat_id, user_id) VALUES
(8, 19);

-- =====================================================
-- 5. Создание групповых чатов
-- =====================================================
-- Общий чат для всех
INSERT INTO chats (name, type, created_by) VALUES
('Общий чат', 'Group', 1);

-- Чат для руководства
INSERT INTO chats (name, type, created_by) VALUES
('Руководство', 'Group', 17);

-- =====================================================
-- 6. Добавление участников в групповые чаты
-- =====================================================
-- Общий чат (chat_id = 9) - все пользователи
INSERT INTO chat_participants (chat_id, user_id) VALUES
(9, 1), (9, 2), (9, 3), (9, 4), (9, 5), (9, 6), (9, 7), (9, 8), (9, 9),
(9, 10), (9, 11), (9, 12), (9, 13), (9, 14), (9, 15), (9, 16), (9, 17), (9, 18), (9, 19);

-- Чат руководства (chat_id = 10) - только руководители
INSERT INTO chat_participants (chat_id, user_id) VALUES
(10, 5), (10, 8), (10, 10), (10, 12), (10, 15), (10, 17), (10, 19);

-- =====================================================
-- 7. Добавление тестовых сообщений
-- =====================================================
-- Сообщения в ИТ отделе
INSERT INTO messages (chat_id, sender_id, text, sent_at) VALUES
(1, 1, 'Всем привет! Как проходит спринт?', datetime('now', '-2 hours')),
(1, 2, 'Привет! У нас всё отлично, задачи движутся по плану', datetime('now', '-115 minutes')),
(1, 3, 'Тестирование нового функционала идёт полным ходом', datetime('now', '-110 minutes')),
(1, 1, 'Отлично! Сегодня в 18:00 планирование следующего спринта', datetime('now', '-100 minutes')),
(1, 4, 'Сервера обновил, всё работает стабильно', datetime('now', '-90 minutes'));

-- Сообщения в Производстве
INSERT INTO messages (chat_id, sender_id, text, sent_at) VALUES
(2, 5, 'Коллеги, на следующей неделе плановый ремонт оборудования', datetime('now', '-5 hours')),
(2, 6, 'Понял, подготовлю график работ', datetime('now', '-280 minutes')),
(2, 7, 'Нужны запчасти для станка, когда заказ?', datetime('now', '-200 minutes'));

-- Сообщения в HR
INSERT INTO messages (chat_id, sender_id, text, sent_at) VALUES
(3, 8, 'Ищем разработчика, может кто порекомендовать?', datetime('now', '-3 hours')),
(3, 9, 'Разместила вакансию на сайтах, уже есть отклики', datetime('now', '-170 minutes'));

-- Сообщения в общем чате
INSERT INTO messages (chat_id, sender_id, text, sent_at) VALUES
(9, 17, 'Уважаемые коллеги! В пятницу корпоратив в 18:00', datetime('now', '-1 day')),
(9, 18, 'Нужно подтвердить участие до четверга', datetime('now', '-23 hours')),
(9, 1, 'Отлично! Будем', datetime('now', '-22 hours')),
(9, 5, 'С производства будем', datetime('now', '-21 hours')),
(9, 8, 'HR отдел в сборе', datetime('now', '-20 hours')),
(9, 12, 'Отдел продаж будет', datetime('now', '-19 hours')),
(9, 15, 'Логистика присоединится', datetime('now', '-18 hours'));

-- =====================================================
-- 8. Обновление статусов пользователей (онлайн)
-- =====================================================
INSERT INTO user_status (user_id, is_online, last_seen) VALUES
(1, 1, datetime('now')),
(2, 1, datetime('now')),
(3, 0, datetime('now', '-30 minutes')),
(4, 1, datetime('now')),
(5, 1, datetime('now')),
(6, 0, datetime('now', '-2 hours')),
(7, 0, datetime('now', '-1 hour')),
(8, 1, datetime('now')),
(9, 1, datetime('now')),
(10, 1, datetime('now')),
(11, 0, datetime('now', '-3 hours')),
(12, 1, datetime('now')),
(13, 1, datetime('now')),
(14, 0, datetime('now', '-45 minutes')),
(15, 1, datetime('now')),
(16, 0, datetime('now', '-5 hours')),
(17, 1, datetime('now')),
(18, 1, datetime('now')),
(19, 0, datetime('now', '-15 minutes'));

-- =====================================================
-- 9. Отметки о прочитанных сообщениях
-- =====================================================
-- Иван прочитал все сообщения в ИТ отделе до 5-го
INSERT INTO user_chat_read (user_id, chat_id, last_read_message_id, read_at) VALUES
(1, 1, 5, datetime('now', '-80 minutes'));

-- Петр прочитал до 4-го
INSERT INTO user_chat_read (user_id, chat_id, last_read_message_id, read_at) VALUES
(2, 1, 4, datetime('now', '-85 minutes'));

-- Директор прочитал все в общем чате
INSERT INTO user_chat_read (user_id, chat_id, last_read_message_id, read_at) VALUES
(17, 9, 7, datetime('now', '-17 hours'));

-- =====================================================
-- Конец скрипта
-- =====================================================