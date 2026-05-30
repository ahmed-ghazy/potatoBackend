-- Personal accounts data migration: local -> production
-- Generated 2026-05-26 19:20:57
-- 1 PersonalAccount + 18 PersonalAccountEntries (TenantId=4 'شركه احمد')

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Safety: refuse to run if production already has data
IF EXISTS (SELECT 1 FROM PersonalAccounts) OR EXISTS (SELECT 1 FROM PersonalAccountEntries)
BEGIN
  RAISERROR('Production already has personal accounts data. Aborting to avoid duplicates.', 16, 1);
  ROLLBACK; RETURN;
END;

SET IDENTITY_INSERT PersonalAccounts ON;
INSERT INTO PersonalAccounts (Id, Name, PhoneNumber, Notes, TenantId, CreatedAt, CreatedBy) VALUES (1, N'محمد و محمود', NULL, NULL, 4, '2026-05-26T14:46:28.346', '4a7c20fc-f244-479e-b962-4eab7503c33c');
SET IDENTITY_INSERT PersonalAccounts OFF;

SET IDENTITY_INSERT PersonalAccountEntries ON;
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (1, 1, '2026-05-11T00:00:00.000', 1, 5700.00, 0.00, NULL, N'300 كيلو بطاطس من الانفار -600 انفار عبوة البطاطس - 1000 تنزيل العربيات - 200 دخان الانفار', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:48:03.246', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (2, 1, '2026-05-11T00:00:00.000', 1, 3600.00, 0.00, NULL, N'انفار فكريه', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:48:34.701', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (3, 1, '2026-05-11T00:00:00.000', 1, 2500.00, 0.00, NULL, N'مرش العربيه لمحمود', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:49:09.941', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (4, 1, '2026-05-18T00:00:00.000', 1, 10000.00, 0.00, NULL, N'عربون شيفا', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:49:33.535', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (5, 1, '2026-05-25T00:00:00.000', 1, 25000.00, 0.00, NULL, N'محمود فوده', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:50:03.324', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (6, 1, '2026-05-20T00:00:00.000', 1, 98500.00, 0.00, NULL, N'لمعروف العيارى', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:50:30.129', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (7, 1, '2026-05-11T00:00:00.000', 2, 15000.00, 0.00, 1, N'عربون لمحمود فوده', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:51:00.917', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (8, 1, '2026-05-19T00:00:00.000', 2, 2000.00, 0.00, 1, N'عربون حمادة البجورى', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:51:29.245', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (9, 1, '2026-05-21T00:00:00.000', 2, 8600.00, 0.00, 1, N'محمد دفعهم لمعروف باقى حساب الارض اللى جبهالى', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:52:19.040', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (10, 1, '2026-05-26T00:00:00.000', 2, 25000.00, 0.00, 1, N'اخدهم لشكاير التلاجه', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:52:51.489', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (11, 1, '2026-05-26T00:00:00.000', 2, 4200.00, 0.00, 1, N'محمد جاب شكاير للخزين', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:53:19.909', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (12, 1, '2026-05-26T00:00:00.000', 2, 100.00, 0.00, 1, N'باقى حساب خالد عبدالهادى', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:53:42.168', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (13, 1, '2026-05-26T00:00:00.000', 2, 700.00, 0.00, 1, N'نفر بطاطس', '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:54:02.326', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (14, 1, '2026-05-26T00:00:00.000', 2, 60000.00, 600.00, 2, NULL, '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:54:41.553', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (15, 1, '2026-05-26T00:00:00.000', 2, 80000.00, 800.00, 2, NULL, '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:55:04.122', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (16, 1, '2026-05-26T00:00:00.000', 2, 60000.00, 0.00, 4, NULL, '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:55:28.459', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (17, 1, '2026-05-26T00:00:00.000', 2, 149480.00, 0.00, 3, NULL, '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:55:52.339', '4a7c20fc-f244-479e-b962-4eab7503c33c');
INSERT INTO PersonalAccountEntries (Id, PersonalAccountId, Date, Direction, Amount, Fee, PaymentMethod, Description, UserId, TenantId, CreatedAt, CreatedBy) VALUES (18, 1, '2026-05-26T00:00:00.000', 2, 50000.00, 0.00, 3, NULL, '4a7c20fc-f244-479e-b962-4eab7503c33c', 4, '2026-05-26T14:56:14.087', '4a7c20fc-f244-479e-b962-4eab7503c33c');
SET IDENTITY_INSERT PersonalAccountEntries OFF;

COMMIT TRANSACTION;
PRINT 'Migration complete: 1 accounts, 18 entries.';

