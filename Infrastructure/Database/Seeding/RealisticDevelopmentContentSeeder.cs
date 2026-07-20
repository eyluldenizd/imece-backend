using Infrastructure.Database.Connections;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Database.Seeding;

public sealed class RealisticDevelopmentContentSeeder : IRealisticDevelopmentContentSeeder
{
    private const int PrimaryCompanyId = 1001;
    private const int SecondaryCompanyId = 2002;

    private readonly IDbExecutor _executor;
    private readonly ILogger<RealisticDevelopmentContentSeeder> _logger;

    public RealisticDevelopmentContentSeeder(
        IDbExecutor executor,
        ILogger<RealisticDevelopmentContentSeeder> logger)
    {
        _executor = executor;
        _logger = logger;
    }

    public async Task SeedAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int commandTimeoutSeconds,
        string seedVersion,
        CancellationToken cancellationToken = default)
    {
        var appliedVersion = await DevelopmentSeedState.GetAppliedVersionAsync(
            _executor,
            connection,
            commandTimeoutSeconds,
            transaction,
            cancellationToken);

        if (string.Equals(appliedVersion, seedVersion, StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Gerçekçi içerik seed atlandı; sürüm '{SeedVersion}' zaten uygulanmış.",
                seedVersion);
            return;
        }

        var adminUserId = await GetUserIdByUsernameAsync(
            connection,
            transaction,
            commandTimeoutSeconds,
            "admin",
            cancellationToken);

        if (adminUserId is null)
        {
            _logger.LogWarning("Admin kullanıcısı bulunamadı; gerçekçi içerik seed atlandı.");
            return;
        }

        await SeedDishesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedServicesAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedEmergencyNumbersAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCorporateAppsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCommunicationChannelsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedAnnouncementsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedCampaignsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedEventsAsync(connection, transaction, commandTimeoutSeconds, adminUserId.Value, cancellationToken);
        await SeedUpcomingEventsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedSocialActivitiesAsync(connection, transaction, commandTimeoutSeconds, adminUserId.Value, cancellationToken);
        await SeedECardsAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedTodayInHistoryAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedMediaAsync(connection, transaction, commandTimeoutSeconds, adminUserId.Value, cancellationToken);
        await SeedServiceTransportAsync(connection, transaction, commandTimeoutSeconds, cancellationToken);
        await SeedMeetingRoomsAndReservationsAsync(
            connection,
            transaction,
            commandTimeoutSeconds,
            adminUserId.Value,
            cancellationToken);
        await SeedWeeklyMenusAsync(connection, transaction, commandTimeoutSeconds, adminUserId.Value, cancellationToken);

        _logger.LogInformation("Gerçekçi development içerik seed uygulandı (sürüm={SeedVersion}).", seedVersion);
    }

    private async Task<int?> GetUserIdByUsernameAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        string username,
        CancellationToken cancellationToken)
    {
        var scalar = await _executor.ExecuteScalarAsync(
            connection,
            "SELECT TOP (1) user_id FROM [dbo].[users] WHERE username = @Username",
            parameters: [new SqlParameter("@Username", username)],
            transaction: transaction,
            commandTimeoutSeconds: timeout,
            cancellationToken: cancellationToken);

        return scalar is null or DBNull ? null : Convert.ToInt32(scalar);
    }

    private async Task SeedDishesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var dishes = new (string Name, string CategoryCode, string Description)[]
        {
            ("Mercimek Çorbası", "corba", "Geleneksel kırmızı mercimek çorbası"),
            ("Ezogelin Çorbası", "corba", "Bulgur ve mercimek karışımı"),
            ("Tavuk Suyu Çorbası", "corba", "Ev yapımı tavuk suyu ile"),
            ("Sebze Çorbası", "corba", "Mevsim sebzeleri ile"),
            ("İşkembe Çorbası", "corba", "Sıcak servis"),
            ("Kuru Fasulye", "ana-yemek", "Pilav eşliğinde"),
            ("Tas Kebabı", "ana-yemek", "Fırında sebzeli"),
            ("Izgara Köfte", "ana-yemek", "Domates biber ile"),
            ("Tavuk Şinitzel", "ana-yemek", "Patates kızartması ile"),
            ("Balık Izgara", "ana-yemek", "Mevsim balığı"),
            ("Mantı", "ana-yemek", "Yoğurt ve sos ile"),
            ("Karnıyarık", "ana-yemek", "Kıymalı patlıcan"),
            ("Et Sote", "ana-yemek", "Sebzeli"),
            ("Tavuk Güveç", "ana-yemek", "Fırında"),
            ("Pirzola", "ana-yemek", "Izgara"),
            ("Makarna Napolitan", "ana-yemek", "Domates soslu"),
            ("Pilav", "yardimci-yemek", "Tereyağlı"),
            ("Bulgur Pilavı", "yardimci-yemek", "Domatesli"),
            ("Mevsim Salata", "salata", "Zeytinyağlı"),
            ("Çoban Salata", "salata", "Taze sebzeler"),
            ("Mevsim Meyve", "tatli", "Günün meyve tabağı"),
            ("Sütlaç", "tatli", "Fırında"),
            ("Kazandibi", "tatli", "Karamelize"),
            ("Revani", "tatli", "Şerbetli"),
            ("Profiterol", "tatli", "Çikolata soslu"),
            ("Ayran", "icecek", "Soğuk servis"),
            ("Limonata", "icecek", "Taze sıkım"),
            ("Meyve Suyu", "icecek", "Karışık"),
            ("Çay", "icecek", "Demleme"),
            ("Kahve", "icecek", "Türk kahvesi"),
            ("Zeytinyağlı Enginar", "ana-yemek", "Mevsim"),
            ("Fırın Sebze", "yardimci-yemek", "Karışık"),
            ("Humus", "salata", "Nohut ezmesi"),
            ("Cacık", "salata", "Yoğurtlu"),
            ("Börek", "ana-yemek", "Peynirli"),
            ("Lahmacun", "ana-yemek", "Fırından"),
            ("Pide", "ana-yemek", "Kaşarlı"),
            ("Döner", "ana-yemek", "Tavuk"),
            ("Tantuni", "ana-yemek", "Acılı"),
            ("Köfte Patates", "ana-yemek", "Fırın"),
            ("Sebze Gratin", "yardimci-yemek", "Beşamel soslu"),
            ("Yoğurt", "yardimci-yemek", "Sade"),
            ("Turşu", "yardimci-yemek", "Karışık"),
            ("Zeytin", "yardimci-yemek", "Siyah-yeşil"),
            ("Helva", "tatli", "Un helvası"),
            ("Baklava", "tatli", "Antep fıstıklı"),
            ("Künefe", "tatli", "Peynirli"),
            ("Güveçte Sebze", "ana-yemek", "Zeytinyağlı"),
            ("Nohut Yemeği", "ana-yemek", "Etli"),
            ("Barbunya Pilaki", "ana-yemek", "Zeytinyağlı")
        };

        foreach (var (name, categoryCode, description) in dishes)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[dishes] WHERE dish_name = @Name)
                BEGIN
                    INSERT INTO [dbo].[dishes]
                    (
                        dish_name, dish_category_id, category, description,
                        is_active, created_at, updated_at
                    )
                    SELECT
                        @Name,
                        dc.dish_category_id,
                        dc.name,
                        @Description,
                        CASE WHEN ABS(CHECKSUM(NEWID())) % 10 > 0 THEN 1 ELSE 0 END,
                        DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 180, SYSUTCDATETIME()),
                        SYSUTCDATETIME()
                    FROM [dbo].[dish_categories] AS dc
                    WHERE dc.code = @CategoryCode;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@CategoryCode", categoryCode),
                    new SqlParameter("@Description", description)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedServicesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var services = new (string Name, string Description, bool Global)[]
        {
            ("İK Self Servis", "İzin, bordro ve özlük işlemleri", false),
            ("BT Destek", "Donanım ve yazılım talepleri", false),
            ("Seyahat Talebi", "İş seyahati planlama", false),
            ("Masraf Bildirimi", "Harcama ve avans süreçleri", false),
            ("Eğitim Kataloğu", "Kurumsal eğitim programları", false),
            ("Sağlık Randevusu", "Anlaşmalı kurum randevuları", false),
            ("Servis Rezervasyonu", "Ring ve servis güzergahları", false),
            ("Yemek Menüsü", "Haftalık yemek listesi", false),
            ("Toplantı Odası", "Oda rezervasyon sistemi", false),
            ("Duyuru Merkezi", "Kurumsal iletişim", false),
            ("Kütüphane", "Dijital doküman arşivi", false),
            ("İletişim Rehberi", "Departman ve kişi rehberi", false),
            ("Acil Durum", "Acil numara ve prosedürler", false),
            ("Sosyal Etkinlikler", "Kulüp ve etkinlik takvimi", false),
            ("Kampanyalar", "Çalışan avantaj programları", false),
            ("Medya Arşivi", "Fotoğraf ve video galerisi", false),
            ("Kurumsal Uygulamalar", "Entegre iş uygulamaları", false),
            ("Organizasyon Şeması", "Şirket yapısı ve birimler", true)
        };

        for (var i = 0; i < services.Length; i++)
        {
            var (name, description, global) = services[i];
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[services] WHERE name = @Name)
                BEGIN
                    INSERT INTO [dbo].[services]
                    (
                        name, description, icon, is_active,
                        company_scope, company_id, branch_scope, department_scope,
                        created_at, updated_at
                    )
                    VALUES
                    (
                        @Name, @Description, N'icon-service', @IsActive,
                        @CompanyScope, @CompanyId, N'All', N'All',
                        SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Description", description),
                    new SqlParameter("@IsActive", i % 7 != 0),
                    new SqlParameter("@CompanyScope", global ? "All" : "Single"),
                    new SqlParameter("@CompanyId", global ? DBNull.Value : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedEmergencyNumbersAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var numbers = new (string Name, string Phone, string Category)[]
        {
            ("Güvenlik Merkezi", "0312 555 0100", "Güvenlik"),
            ("Sağlık Birimi", "0312 555 0101", "Sağlık"),
            ("İnsan Kaynakları", "0312 555 0102", "İK"),
            ("Bilgi Teknolojileri", "0312 555 0103", "BT"),
            ("Tesis Yönetimi", "0312 555 0104", "Tesis"),
            ("Yangın İhbar", "110", "Acil"),
            ("Ambulans", "112", "Acil"),
            ("Polis İmdat", "155", "Acil"),
            ("Doğalgaz Acil", "187", "Altyapı"),
            ("Elektrik Arıza", "186", "Altyapı"),
            ("Su Arıza", "185", "Altyapı"),
            ("Çevre ve İSG", "0312 555 0110", "İSG"),
            ("Hukuk Müşavirliği", "0312 555 0111", "Hukuk"),
            ("Kurumsal İletişim", "0312 555 0112", "İletişim"),
            ("Lojistik Koordinasyon", "0312 555 0113", "Operasyon"),
            ("Reception", "0312 555 0114", "Resepsiyon"),
            ("Otopark Yönetimi", "0312 555 0115", "Tesis"),
            ("Yemekhane Sorumlusu", "0312 555 0116", "Tesis")
        };

        for (var i = 0; i < numbers.Length; i++)
        {
            var (name, phone, category) = numbers[i];
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[emergency_numbers] WHERE name = @Name)
                BEGIN
                    INSERT INTO [dbo].[emergency_numbers]
                    (
                        name, phone_number, category, description, is_active, display_order,
                        company_scope, company_id, branch_scope, department_scope,
                        created_at, updated_at
                    )
                    VALUES
                    (
                        @Name, @Phone, @Category, @Description, @IsActive, @DisplayOrder,
                        N'Single', @CompanyId, N'All', N'All',
                        SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Phone", phone),
                    new SqlParameter("@Category", category),
                    new SqlParameter("@Description", $"{name} iletişim hattı"),
                    new SqlParameter("@IsActive", i % 9 != 0),
                    new SqlParameter("@DisplayOrder", i + 1),
                    new SqlParameter("@CompanyId", PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedCorporateAppsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var apps = new (string Title, string CategoryName, string Url)[]
        {
            ("Bordro Görüntüleme", "HR", "https://intranet.atlas.local/hr/payroll"),
            ("İzin Yönetimi", "HR", "https://intranet.atlas.local/hr/leave"),
            ("Performans Değerlendirme", "HR", "https://intranet.atlas.local/hr/performance"),
            ("İşe Alım Portalı", "HR", "https://intranet.atlas.local/hr/recruitment"),
            ("Eğitim Yönetim Sistemi", "HR", "https://intranet.atlas.local/hr/lms"),
            ("Varlık Yönetimi", "IT", "https://intranet.atlas.local/it/assets"),
            ("Yardım Masası", "IT", "https://intranet.atlas.local/it/helpdesk"),
            ("VPN Erişim", "IT", "https://intranet.atlas.local/it/vpn"),
            ("Siber Güvenlik Portalı", "IT", "https://intranet.atlas.local/it/security"),
            ("Bulut Depolama", "IT", "https://intranet.atlas.local/it/storage"),
            ("Masraf Yönetimi", "Finance", "https://intranet.atlas.local/finance/expenses"),
            ("Satın Alma", "Finance", "https://intranet.atlas.local/finance/procurement"),
            ("Bütçe Planlama", "Finance", "https://intranet.atlas.local/finance/budget"),
            ("Fatura Onay", "Finance", "https://intranet.atlas.local/finance/invoices"),
            ("Sözleşme Arşivi", "Finance", "https://intranet.atlas.local/finance/contracts"),
            ("Proje Takip", "Operations", "https://intranet.atlas.local/ops/projects"),
            ("Depo Yönetimi", "Operations", "https://intranet.atlas.local/ops/warehouse"),
            ("Kalite Yönetimi", "Operations", "https://intranet.atlas.local/ops/quality"),
            ("Üretim Planlama", "Operations", "https://intranet.atlas.local/ops/planning"),
            ("Lojistik Takip", "Operations", "https://intranet.atlas.local/ops/logistics"),
            ("Enerji İzleme", "Operations", "https://intranet.nova.local/ops/energy"),
            ("Bakım Yönetimi", "Operations", "https://intranet.nova.local/ops/maintenance"),
            ("Vardiya Planlama", "Operations", "https://intranet.nova.local/ops/shifts"),
            ("Raporlama Merkezi", "Finance", "https://intranet.atlas.local/finance/reports"),
            ("Kurumsal Wiki", "IT", "https://intranet.atlas.local/it/wiki")
        };

        for (var i = 0; i < apps.Length; i++)
        {
            var (title, categoryName, url) = apps[i];
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[corporate_apps] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[corporate_apps]
                    (
                        title, description, url, corporate_app_category_id, category,
                        icon_url, is_active,
                        company_scope, company_id, branch_scope, department_scope
                    )
                    SELECT
                        @Title, @Description, @Url, c.corporate_app_category_id, c.name,
                        N'/assets/icons/app.svg', @IsActive,
                        N'Single', @CompanyId, N'All', N'All'
                    FROM [dbo].[corporate_app_categories] AS c
                    WHERE c.name = @CategoryName;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", $"{title} uygulamasına hızlı erişim"),
                    new SqlParameter("@Url", url),
                    new SqlParameter("@CategoryName", categoryName),
                    new SqlParameter("@IsActive", i % 8 != 0),
                    new SqlParameter("@CompanyId", i % 5 == 0 ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedCommunicationChannelsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var channels = new (string Name, string TypeCode, string Address)[]
        {
            ("Atlas LinkedIn", "linkedin", "https://linkedin.com/company/atlas-teknoloji"),
            ("Atlas Instagram", "instagram", "https://instagram.com/atlasteknoloji"),
            ("Atlas YouTube", "youtube", "https://youtube.com/@atlasteknoloji"),
            ("Atlas X", "x", "https://x.com/atlasteknoloji"),
            ("Nova LinkedIn", "linkedin", "https://linkedin.com/company/nova-enerji"),
            ("Nova Instagram", "instagram", "https://instagram.com/novaenerji"),
            ("İK E-posta", "email", "mailto:ik@atlas.com.tr"),
            ("BT Destek E-posta", "email", "mailto:destek@atlas.com.tr"),
            ("Kurumsal İletişim", "email", "mailto:iletisim@atlas.com.tr"),
            ("Genel Telefon", "phone", "tel:+903125550000"),
            ("Ankara Santral", "phone", "tel:+903125551000"),
            ("İzmir Santral", "phone", "tel:+902325550000"),
            ("Kariyer Sayfası", "web", "https://kariyer.atlas.com.tr"),
            ("Kurumsal Web", "web", "https://www.atlas.com.tr"),
            ("Nova Kurumsal Web", "web", "https://www.novaenerji.com.tr"),
            ("WhatsApp İK Hattı", "whatsapp", "https://wa.me/905551112233"),
            ("YouTube Eğitim", "youtube", "https://youtube.com/@atlas-akademi"),
            ("Facebook Etkinlik", "facebook", "https://facebook.com/atlas.etkinlik"),
            ("X Teknoloji Blog", "x", "https://x.com/atlas_tech_blog"),
            ("LinkedIn Nova Kariyer", "linkedin", "https://linkedin.com/company/nova-enerji/jobs"),
            ("Instagram Nova Sürdürülebilirlik", "instagram", "https://instagram.com/nova.surdurulebilirlik"),
            ("Web Sürdürülebilirlik Raporu", "web", "https://www.novaenerji.com.tr/surdurulebilirlik"),
            ("E-posta Basın", "email", "mailto:basin@novaenerji.com.tr"),
            ("Telefon Acil Hat", "phone", "tel:+908505550000"),
            ("Web Yatırımcı İlişkileri", "web", "https://www.novaenerji.com.tr/yatirimci")
        };

        for (var i = 0; i < channels.Length; i++)
        {
            var (name, typeCode, address) = channels[i];
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[communication_channels] WHERE channel_name = @Name)
                BEGIN
                    INSERT INTO [dbo].[communication_channels]
                    (
                        channel_name, type, communication_channel_type_id, address_url,
                        department_in_charge, description, sort_order, is_active,
                        company_scope, company_id, branch_scope, department_scope,
                        created_at, updated_at
                    )
                    SELECT
                        @Name, t.code, t.communication_channel_type_id, @Address,
                        @Department, @Description, @SortOrder, @IsActive,
                        N'Single', @CompanyId, N'All', N'All',
                        SYSUTCDATETIME(), SYSUTCDATETIME()
                    FROM [dbo].[communication_channel_types] AS t
                    WHERE t.code = @TypeCode;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@TypeCode", typeCode),
                    new SqlParameter("@Address", address),
                    new SqlParameter("@Department", i % 3 == 0 ? "Kurumsal İletişim" : "İnsan Kaynakları"),
                    new SqlParameter("@Description", $"{name} resmi iletişim kanalı"),
                    new SqlParameter("@SortOrder", i + 1),
                    new SqlParameter("@IsActive", i % 11 != 0),
                    new SqlParameter("@CompanyId", name.StartsWith("Nova", StringComparison.Ordinal) ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedAnnouncementsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var titles = new[]
        {
            "Esnek Çalışma Modeli Güncellemesi",
            "Yıllık Performans Değerlendirme Takvimi",
            "Şirket Pikniği Kayıtları Başladı",
            "Yeni Yemekhane Menüsü Yayında",
            "Bilgi Güvenliği Farkındalık Eğitimi",
            "Sağlık Taraması Randevu Duyurusu",
            "Kış Lastiği Destek Programı",
            "Ofis Taşınma Planı Hakkında",
            "Yeni İK Self Servis Portalı",
            "Kurumsal Eğitim Kataloğu Güncellendi",
            "Enerji Tasarrufu Haftası Etkinlikleri",
            "Yıl Sonu Tatil Planlaması",
            "Aile Günü Etkinlik Programı",
            "Yeni Toplantı Odası Rezervasyon Sistemi",
            "Servis Güzergah Değişikliği",
            "Doğum Günü Kutlama Köşesi",
            "Mentorluk Programına Başvurular",
            "Sürdürülebilirlik Raporu Yayınlandı",
            "Yeni Çalışan Oryantasyon Programı",
            "Kış Konseri Davetiyeleri",
            "Mobil Uygulama Güncellemesi",
            "Acil Durum Tatbikatı Duyurusu",
            "Şirket Kütüphanesi Yenilendi",
            "Yemek Kartı Bakiye Yükleme",
            "Kurumsal İletişim Kanalları Güncellendi",
            "Global Duyuru: Yılın Projesi Seçildi"
        };

        for (var i = 0; i < titles.Length; i++)
        {
            var global = i == titles.Length - 1;
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[announcements] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[announcements]
                    (
                        company_id, scope_type, title, content, is_pinned,
                        publish_start, publish_end, view_count, created_at, updated_at
                    )
                    VALUES
                    (
                        @CompanyId, @ScopeType, @Title, @Content, @IsPinned,
                        @PublishStart, @PublishEnd, @ViewCount,
                        SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", titles[i]),
                    new SqlParameter("@Content", $"{titles[i]} hakkında ayrıntılı bilgi için intranet duyurular bölümünü ziyaret edin."),
                    new SqlParameter("@IsPinned", i < 3),
                    new SqlParameter("@PublishStart", DateTime.UtcNow.AddDays(-30 + i)),
                    new SqlParameter("@PublishEnd", i % 4 == 0 ? DBNull.Value : DateTime.UtcNow.AddDays(60 + i)),
                    new SqlParameter("@ViewCount", 10 + i * 3),
                    new SqlParameter("@ScopeType", global ? "Global" : "Company"),
                    new SqlParameter("@CompanyId", global ? DBNull.Value : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedCampaignsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var titles = new[]
        {
            "Yaz Sağlık Taraması",
            "Eğitim Burs Programı",
            "Spor Salonu Üyelik İndirimi",
            "Aile Sigortası Avantajı",
            "Teknoloji Fuarı Katılımı",
            "Gönüllülük Günü",
            "Kitap Kulübü Üyeliği",
            "Doğa Yürüyüşü Organizasyonu",
            "Kış Mont Destek Programı",
            "Çocuk Bakım Desteği",
            "Ulaşım Kartı Desteği",
            "Yemek Kartı Promosyonu",
            "Konser Bilet Kampanyası",
            "Online Eğitim Paketi",
            "Referans Programı Ödülleri"
        };

        for (var i = 0; i < titles.Length; i++)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[campaigns] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[campaigns]
                    (
                        title, description, image_url, target_url,
                        start_date, end_date, is_active,
                        company_scope, company_id, branch_scope, department_scope
                    )
                    VALUES
                    (
                        @Title, @Description, @ImageUrl, @TargetUrl,
                        @StartDate, @EndDate, @IsActive,
                        N'Single', @CompanyId, N'All', N'All'
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", titles[i]),
                    new SqlParameter("@Description", $"{titles[i]} kampanyası çalışanlarımız için hazırlandı."),
                    new SqlParameter("@ImageUrl", $"/uploads/global/campaigns/kampanya-{i + 1}.jpg"),
                    new SqlParameter("@TargetUrl", $"https://intranet.atlas.local/kampanyalar/{i + 1}"),
                    new SqlParameter("@StartDate", DateTime.UtcNow.AddDays(-15 + i * 2)),
                    new SqlParameter("@EndDate", DateTime.UtcNow.AddDays(30 + i * 5)),
                    new SqlParameter("@IsActive", i % 6 != 0),
                    new SqlParameter("@CompanyId", i % 4 == 0 ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedEventsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        int createdBy,
        CancellationToken cancellationToken)
    {
        var events = new (string Title, string Location, int DayOffset)[]
        {
            ("Yıllık Strateji Toplantısı", "Ankara Genel Müdürlük", -20),
            ("Teknoloji Zirvesi", "İstanbul Kongre Merkezi", 14),
            ("Yaz Koşusu", "Gençlik Parkı", 45),
            ("Liderlik Atölyesi", "Eğitim Salonu A", 7),
            ("Kurumsal Tanıtım Günü", "Fuaye Alanı", 3),
            ("Ar-Ge Proje Sunumu", "Toplantı Odası 301", -5),
            ("Enerji Verimliliği Semineri", "Konferans Salonu", 21),
            ("Yıl Sonu Değerlendirme", "Ankara Genel Müdürlük", 90),
            ("İnovasyon Hackathon", "Ar-Ge Merkezi", 30),
            ("Sağlık ve Wellness Günü", "Spor Salonu", 10),
            ("Müşteri Memnuniyeti Workshop", "İzmir Bölge Ofisi", 18),
            ("Sürdürülebilirlik Paneli", "Online", 25),
            ("Yeni Mezun Buluşması", "Kariyer Merkezi", 12),
            ("Kış Festivali", "Şirket Bahçesi", 60),
            ("Bursiyer Tanışma Toplantısı", "Eğitim Salonu B", 8),
            ("Proje Lansmanı", "Ana Sahne", 5),
            ("Departmanlar Arası Turnuva", "Spor Kompleksi", 35),
            ("Yönetim Kurulu Bilgilendirme", "Yönetim Katı", -10)
        };

        for (var i = 0; i < events.Length; i++)
        {
            var (title, location, dayOffset) = events[i];
            var start = DateTime.UtcNow.Date.AddDays(dayOffset).AddHours(9 + i % 4);
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[events] WHERE title = @Title AND start_datetime = @Start)
                BEGIN
                    INSERT INTO [dbo].[events]
                    (
                        company_id, scope_type, branch_scope, department_scope,
                        title, description, event_type, location,
                        start_datetime, end_datetime, is_all_day, created_by, created_at
                    )
                    VALUES
                    (
                        @CompanyId, N'Company', N'All', N'All',
                        @Title, @Description, @EventType, @Location,
                        @Start, @End, 0, @CreatedBy, SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", $"{title} etkinliğine tüm çalışanlar davetlidir."),
                    new SqlParameter("@EventType", i % 3 == 0 ? "Konferans" : "Toplantı"),
                    new SqlParameter("@Location", location),
                    new SqlParameter("@Start", start),
                    new SqlParameter("@End", start.AddHours(2 + i % 3)),
                    new SqlParameter("@CreatedBy", createdBy),
                    new SqlParameter("@CompanyId", i % 5 == 0 ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedUpcomingEventsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var upcoming = new (string Title, string Location, int DayOffset)[]
        {
            ("Teknoloji Buluşması", "İstanbul Genel Merkez", 3),
            ("Yeni Çalışan Oryantasyonu", "Eğitim Salonu A", 5),
            ("Liderlik Atölyesi", "Konferans Salonu", 7),
            ("Aile Pikniği", "Şirket Bahçesi", 14),
            ("Kurumsal Koşu Etkinliği", "Gençlik Parkı", 10),
            ("Kariyer Gelişim Semineri", "Ankara Bölge Müdürlüğü", 12),
            ("Bilgi Güvenliği Farkındalık Günü", "Online", 4),
            ("Proje Lansman Sunumu", "Ana Sahne", 6),
            ("Departmanlar Arası Voleybol Turnuvası", "Spor Kompleksi", 18),
            ("Sürdürülebilirlik Paneli", "Konferans Salonu B", 21),
            ("Yaz Festivali Hazırlık Toplantısı", "Fuaye Alanı", 2),
            ("Mentorluk Programı Tanıtımı", "Kariyer Merkezi", 9),
            ("Ar-Ge İnovasyon Günü", "Ar-Ge Merkezi", 16),
            ("Sağlıklı Yaşam Workshop", "Wellness Odası", 11),
            ("Kurumsal Sosyal Sorumluluk Günü", "Topluluk Merkezi", 25),
            ("Yönetici Bilgilendirme Oturumu", "Yönetim Katı", 8),
            ("Enerji Verimliliği Semineri", "İzmir Operasyon Merkezi", 15),
            ("Yıl Ortası Değerlendirme", "Ankara Genel Müdürlük", 28)
        };

        for (var i = 0; i < upcoming.Length; i++)
        {
            var (title, location, dayOffset) = upcoming[i];
            var eventDate = DateTime.UtcNow.Date.AddDays(dayOffset).AddHours(10 + i % 5);

            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[upcoming_events] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[upcoming_events]
                    (title, description, event_date, location, is_active)
                    VALUES
                    (@Title, @Description, @EventDate, @Location, @IsActive);
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", $"{title} etkinliği yaklaşan etkinlikler listesinde yer almaktadır."),
                    new SqlParameter("@EventDate", eventDate),
                    new SqlParameter("@Location", location),
                    new SqlParameter("@IsActive", i % 7 != 0)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedSocialActivitiesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        int createdBy,
        CancellationToken cancellationToken)
    {
        var activities = new (string Title, string Type, int DayOffset)[]
        {
            ("Fotoğraf Kulübü Buluşması", "Kulüp", 12),
            ("Satranç Turnuvası", "Turnuva", 20),
            ("Kitap Okuma Grubu", "Kulüp", 8),
            ("Yoga Seansı", "Wellness", 5),
            ("Mutfak Atölyesi", "Workshop", 15),
            ("Doğa Yürüyüşü", "Spor", 25),
            ("Gönüllü Kan Bağışı", "Sosyal Sorumluluk", 18),
            ("Sinema Gecesi", "Eğlence", 10),
            ("Koşu Grubu", "Spor", 6),
            ("Resim Atölyesi", "Workshop", 22),
            ("Müzik Jam Session", "Kulüp", 14),
            ("Aile Pikniği", "Etkinlik", 40)
        };

        for (var i = 0; i < activities.Length; i++)
        {
            var (title, type, dayOffset) = activities[i];
            var start = DateTime.UtcNow.Date.AddDays(dayOffset).AddHours(17);
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[social_activities] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[social_activities]
                    (
                        title, description, activity_type, location,
                        start_at, end_at, company_scope, company_id,
                        branch_scope, department_scope, status, is_active,
                        created_by, created_at, updated_at
                    )
                    VALUES
                    (
                        @Title, @Description, @Type, @Location,
                        @Start, @End, N'Single', @CompanyId,
                        N'All', N'All', @Status, @IsActive,
                        @CreatedBy, SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", $"{title} için kayıtlar açıldı."),
                    new SqlParameter("@Type", type),
                    new SqlParameter("@Location", i % 2 == 0 ? "Sosyal Tesisler" : "Etkinlik Alanı"),
                    new SqlParameter("@Start", start),
                    new SqlParameter("@End", start.AddHours(2)),
                    new SqlParameter("@CompanyId", PrimaryCompanyId),
                    new SqlParameter("@Status", i % 4 == 0 ? "Draft" : "Published"),
                    new SqlParameter("@IsActive", i % 7 != 0),
                    new SqlParameter("@CreatedBy", createdBy)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedECardsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var cards = new[]
        {
            "Hoş Geldiniz",
            "Doğum Gününüz Kutlu Olsun",
            "Yıl Dönümünüz Kutlu Olsun",
            "Tebrikler",
            "Geçmiş Olsun",
            "Yeni Yılınız Kutlu Olsun",
            "Bayramınız Mübarek Olsun",
            "Başarılarınızın Devamını Dileriz",
            "Emekleriniz İçin Teşekkürler",
            "Yeni Görevinizde Başarılar",
            "Aile Gününüz Kutlu Olsun",
            "Projede Başarılar"
        };

        for (var i = 0; i < cards.Length; i++)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[e_cards] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[e_cards]
                    (
                        title, description, card_type, image_url, redirect_url,
                        is_active, display_order,
                        company_scope, company_id, branch_scope, department_scope,
                        created_at, updated_at
                    )
                    VALUES
                    (
                        @Title, @Description, @CardType, @ImageUrl, @RedirectUrl,
                        @IsActive, @DisplayOrder,
                        N'Single', @CompanyId, N'All', N'All',
                        SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", cards[i]),
                    new SqlParameter("@Description", $"{cards[i]} e-kartı ile mesaj gönderin."),
                    new SqlParameter("@CardType", i % 2 == 0 ? "Kutlama" : "Teşekkür"),
                    new SqlParameter("@ImageUrl", $"/uploads/global/ecards/kart-{i + 1}.png"),
                    new SqlParameter("@RedirectUrl", $"https://intranet.atlas.local/ecards/{i + 1}"),
                    new SqlParameter("@IsActive", i % 5 != 0),
                    new SqlParameter("@DisplayOrder", i + 1),
                    new SqlParameter("@CompanyId", PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedTodayInHistoryAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var entries = new (string Title, int Month, int Day)[]
        {
            ("Şirketimizin Kuruluş Yıldönümü", 3, 15),
            ("İlk Ar-Ge Merkezi Açılışı", 6, 2),
            ("Nova Enerji Birleşmesi", 9, 20),
            ("Uluslararası ISO Sertifikası", 11, 8),
            ("Ankara Kampüs Açılışı", 4, 12),
            ("İzmir Bölge Ofisi Faaliyete Geçti", 7, 25),
            ("Enerji Verimliliği Ödülü", 12, 5),
            ("Sürdürülebilirlik Raporu İlk Yayın", 1, 18),
            ("1000. Çalışan Kutlaması", 5, 30),
            ("Mobil Uygulama Lansmanı", 10, 3),
            ("Hackathon Birinciliği", 2, 14),
            ("Kurumsal Akademi Kuruldu", 8, 9),
            ("Yenilenebilir Enerji Yatırımı", 3, 28),
            ("Burs Programı Başlangıcı", 9, 1),
            ("Dijital Dönüşüm Projesi", 6, 18),
            ("Kalite Yönetim Sistemi", 4, 22),
            ("Çevre Dostu Üretim Tesisi", 7, 7),
            ("Kadın Liderlik Programı", 3, 8),
            ("Genç Yetenek Programı", 11, 15),
            ("Uluslararası Fuar Katılımı", 5, 6),
            ("Ar-Ge Patent Başvurusu", 10, 21),
            ("Enerji Depolama Projesi", 12, 12),
            ("Kurumsal Kütüphane Açılışı", 1, 30),
            ("Spor Kulübü Kuruluşu", 2, 28),
            ("Mentorluk Programı Lansmanı", 9, 12)
        };

        for (var i = 0; i < entries.Length; i++)
        {
            var (title, month, day) = entries[i];
            var eventDate = new DateTime(DateTime.UtcNow.Year, month, day, 0, 0, 0, DateTimeKind.Utc);
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[today_in_history] WHERE title = @Title)
                BEGIN
                    INSERT INTO [dbo].[today_in_history]
                    (
                        event_date, title, description, image_url,
                        company_scope, company_id, branch_scope, department_scope,
                        created_at
                    )
                    VALUES
                    (
                        @EventDate, @Title, @Description, @ImageUrl,
                        @CompanyScope, @CompanyId, N'All', N'All',
                        SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", title),
                    new SqlParameter("@Description", $"{title} — kurumsal tarihimizden önemli bir dönüm noktası."),
                    new SqlParameter("@EventDate", eventDate),
                    new SqlParameter("@ImageUrl", $"/uploads/global/history/tarih-{i + 1}.jpg"),
                    new SqlParameter("@CompanyScope", i % 6 == 0 ? "All" : "Single"),
                    new SqlParameter("@CompanyId", i % 6 == 0 ? DBNull.Value : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedMediaAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        int uploadedBy,
        CancellationToken cancellationToken)
    {
        var folderNames = new[]
        {
            "Kurumsal Etkinlikler", "Yıl Sonu Kutlaması", "Eğitim Programları",
            "Saha Ziyaretleri", "Ar-Ge Merkezi", "Sürdürülebilirlik",
            "Spor Etkinlikleri", "Liderlik Zirvesi", "Teknoloji Günleri",
            "Aile Günü", "Proje Lansmanları", "Basın Fotoğrafları",
            "Ofis Yaşamı", "Gönüllülük", "Doğa Etkinlikleri",
            "Kariyer Günleri", "Ödül Törenleri", "Kurumsal Tanıtım"
        };

        for (var i = 0; i < folderNames.Length; i++)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (
                    SELECT 1 FROM [dbo].[media_folders]
                    WHERE folder_name = @FolderName AND company_id = @CompanyId)
                BEGIN
                    INSERT INTO [dbo].[media_folders]
                    (
                        company_id, scope_type, branch_scope, department_scope,
                        folder_name, folder_type, description, is_public, is_active,
                        created_by, created_at, updated_at
                    )
                    VALUES
                    (
                        @CompanyId, N'Company', N'All', N'All',
                        @FolderName, N'Gallery', @Description, @IsPublic, @IsActive,
                        @CreatedBy, SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@FolderName", folderNames[i]),
                    new SqlParameter("@Description", $"{folderNames[i]} fotoğraf arşivi"),
                    new SqlParameter("@IsPublic", i % 4 != 0),
                    new SqlParameter("@IsActive", i % 9 != 0),
                    new SqlParameter("@CreatedBy", uploadedBy),
                    new SqlParameter("@CompanyId", i % 7 == 0 ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }

        for (var i = 1; i <= 42; i++)
        {
            var mediaType = i <= 20 ? "Photo" : i <= 35 ? "Document" : "Image";
            var contentType = mediaType == "Document" ? "application/pdf" : "image/png";
            var extension = mediaType == "Document" ? ".pdf" : ".png";
            var storedFileName = mediaType == "Document" ? $"document-{i}.pdf" : $"placeholder-{i}.png";
            var relativePath = mediaType == "Document"
                ? $"/uploads/global/documents/document-{i}.pdf"
                : $"/uploads/global/media/placeholder-{i}.png";

            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[media_files] WHERE stored_file_name = @StoredFileName)
                BEGIN
                    INSERT INTO [dbo].[media_files]
                    (
                        company_id, scope_type, branch_scope, department_scope,
                        folder_id, media_type, title, description,
                        original_file_name, stored_file_name, file_extension, content_type,
                        relative_path, file_size_bytes, sort_order, is_active,
                        uploaded_by, uploaded_at, updated_at
                    )
                    SELECT TOP (1)
                        @CompanyId, N'Company', N'All', N'All',
                        f.folder_id, @MediaType, @Title, @Description,
                        @OriginalFileName, @StoredFileName, @FileExtension, @ContentType,
                        @RelativePath, @FileSize, @SortOrder, @IsActive,
                        @UploadedBy, SYSUTCDATETIME(), SYSUTCDATETIME()
                    FROM [dbo].[media_folders] AS f
                    WHERE f.company_id = @CompanyId
                    ORDER BY f.folder_id;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", $"Kurumsal Görsel {i}"),
                    new SqlParameter("@Description", i % 3 == 0 ? DBNull.Value : $"Etkinlik görseli #{i}"),
                    new SqlParameter("@OriginalFileName", mediaType == "Document" ? $"dokuman-{i}.pdf" : $"gorsel-{i}.png"),
                    new SqlParameter("@StoredFileName", storedFileName),
                    new SqlParameter("@MediaType", mediaType),
                    new SqlParameter("@ContentType", contentType),
                    new SqlParameter("@FileExtension", extension),
                    new SqlParameter("@RelativePath", relativePath),
                    new SqlParameter("@FileSize", 120_000 + i * 1000),
                    new SqlParameter("@SortOrder", i),
                    new SqlParameter("@IsActive", i % 10 != 0),
                    new SqlParameter("@UploadedBy", uploadedBy),
                    new SqlParameter("@CompanyId", i % 6 == 0 ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedServiceTransportAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        CancellationToken cancellationToken)
    {
        var locationNames = new[]
        {
            "Genel Müdürlük", "Ankara Merkez", "Kızılay Durak", "Söğütözü",
            "İstanbul Ofis", "Levent Metro", "Maslak", "Kadıköy",
            "İzmir Alsancak", "Bornova", "Karşıyaka", "Konak",
            "Bursa Osmangazi", "Nilüfer", "Adana Merkez", "Seyhan",
            "Antalya Lara", "Kepez", "Gaziantep Şehitkamil", "Şehir Merkezi",
            "Etimesgut", "Yenimahalle", "Çankaya", "Ulus",
            "Gebze", "Pendik"
        };

        for (var i = 0; i < locationNames.Length; i++)
        {
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[service_locations] WHERE name = @Name)
                BEGIN
                    INSERT INTO [dbo].[service_locations]
                    (
                        company_id, branch_id, name, service_location_type_id,
                        location_type, address, latitude, longitude,
                        is_active, created_at, updated_at
                    )
                    SELECT
                        @CompanyId, b.branch_id, @Name, t.service_location_type_id,
                        t.name, @Address, @Lat, @Lng,
                        @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME()
                    FROM [dbo].[service_location_types] AS t
                    CROSS JOIN (
                        SELECT TOP (1) branch_id
                        FROM [dbo].[branches]
                        WHERE company_id = @CompanyId
                        ORDER BY branch_id
                    ) AS b
                    WHERE t.name = @LocationType;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", locationNames[i]),
                    new SqlParameter("@Address", $"{locationNames[i]}, Türkiye"),
                    new SqlParameter("@LocationType", i % 3 == 0 ? "Durak" : i % 3 == 1 ? "Ofis" : "Merkez"),
                    new SqlParameter("@Lat", 39.0m + i * 0.05m),
                    new SqlParameter("@Lng", 32.0m + i * 0.04m),
                    new SqlParameter("@IsActive", i % 8 != 0),
                    new SqlParameter("@CompanyId", i % 4 == 0 ? SecondaryCompanyId : PrimaryCompanyId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }

        var routes = new (string Name, string Departure, string Arrival)[]
        {
            ("Sabah Merkez Hattı", "Kızılay Durak", "Genel Müdürlük"),
            ("Akşam Dönüş Hattı", "Genel Müdürlük", "Kızılay Durak"),
            ("Ankara Söğütözü", "Söğütözü", "Genel Müdürlük"),
            ("İstanbul Levent", "Levent Metro", "İstanbul Ofis"),
            ("İstanbul Anadolu", "Kadıköy", "İstanbul Ofis"),
            ("İzmir Bornova", "Bornova", "İzmir Alsancak"),
            ("Bursa Hattı", "Nilüfer", "Bursa Osmangazi"),
            ("Adana Sabah", "Seyhan", "Adana Merkez"),
            ("Antalya Lara", "Lara", "Antalya Lara"),
            ("Gebze-Pendik", "Gebze", "Pendik")
        };

        for (var i = 0; i < routes.Length; i++)
        {
            var (name, departure, arrival) = routes[i];
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[service_routes] WHERE route_name = @Name)
                BEGIN
                    INSERT INTO [dbo].[service_routes]
                    (
                        route_name, departure_location, arrival_location,
                        departure_location_id, arrival_location_id,
                        route_description, departure_time, arrival_time,
                        is_active, display_order, created_at, updated_at
                    )
                    SELECT
                        @Name, @Departure, @Arrival,
                        d.service_location_id, a.service_location_id,
                        @Description, @DepartureTime, @ArrivalTime,
                        @IsActive, @DisplayOrder, SYSUTCDATETIME(), SYSUTCDATETIME()
                    FROM [dbo].[service_locations] AS d
                    CROSS JOIN [dbo].[service_locations] AS a
                    WHERE d.name = @Departure AND a.name = @Arrival;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Departure", departure),
                    new SqlParameter("@Arrival", arrival),
                    new SqlParameter("@Description", $"{name} servis güzergahı"),
                    new SqlParameter("@DepartureTime", TimeSpan.FromHours(7 + i % 3)),
                    new SqlParameter("@ArrivalTime", TimeSpan.FromHours(8 + i % 3)),
                    new SqlParameter("@IsActive", i % 5 != 0),
                    new SqlParameter("@DisplayOrder", i + 1)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);

            var stopCount = 4 + i % 5;
            for (var stop = 1; stop <= stopCount; stop++)
            {
                await _executor.ExecuteNonQueryAsync(
                    connection,
                    """
                    IF NOT EXISTS (
                        SELECT 1
                        FROM [dbo].[service_route_stops] AS s
                        INNER JOIN [dbo].[service_routes] AS r ON r.service_route_id = s.service_route_id
                        WHERE r.route_name = @RouteName AND s.stop_order = @StopOrder)
                    BEGIN
                        INSERT INTO [dbo].[service_route_stops]
                        (
                            service_route_id, service_location_id, stop_order,
                            arrival_time, departure_time, notes, is_active
                        )
                        SELECT
                            r.service_route_id, l.service_location_id, @StopOrder,
                            @ArrivalTime, @DepartureTime, @Notes, 1
                        FROM [dbo].[service_routes] AS r
                        CROSS JOIN (
                            SELECT service_location_id
                            FROM [dbo].[service_locations]
                            ORDER BY service_location_id
                            OFFSET @Offset ROWS FETCH NEXT 1 ROWS ONLY
                        ) AS l
                        WHERE r.route_name = @RouteName;
                    END
                    """,
                    parameters:
                    [
                        new SqlParameter("@RouteName", name),
                        new SqlParameter("@StopOrder", stop),
                        new SqlParameter("@Offset", (i * 3 + stop) % 20),
                        new SqlParameter("@ArrivalTime", TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(stop * 8))),
                        new SqlParameter("@DepartureTime", TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(stop * 8 + 2))),
                        new SqlParameter("@Notes", $"Durak {stop}")
                    ],
                    transaction: transaction,
                    commandTimeoutSeconds: timeout,
                    cancellationToken: cancellationToken);
            }
        }
    }

    private async Task SeedMeetingRoomsAndReservationsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        int organizerUserId,
        CancellationToken cancellationToken)
    {
        for (var i = 1; i <= 18; i++)
        {
            var code = $"ODA-{i:D3}";
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[meeting_rooms] WHERE code = @Code AND company_id = @CompanyId)
                BEGIN
                    INSERT INTO [dbo].[meeting_rooms]
                    (
                        company_id, branch_id, name, code, floor, capacity,
                        location_description, features, is_active, created_at, updated_at
                    )
                    SELECT
                        @CompanyId, b.branch_id, @Name, @Code, @Floor, @Capacity,
                        @Location, @Features, @IsActive, SYSUTCDATETIME(), SYSUTCDATETIME()
                    FROM (
                        SELECT TOP (1) branch_id
                        FROM [dbo].[branches]
                        WHERE company_id = @CompanyId
                        ORDER BY branch_id
                    ) AS b;
                END
                """,
                parameters:
                [
                    new SqlParameter("@CompanyId", i % 4 == 0 ? SecondaryCompanyId : PrimaryCompanyId),
                    new SqlParameter("@Name", $"Toplantı Odası {i}"),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Floor", $"{(i % 5) + 1}. Kat"),
                    new SqlParameter("@Capacity", 4 + i * 2),
                    new SqlParameter("@Location", "Ana bina"),
                    new SqlParameter("@Features", i % 2 == 0 ? "Projeksiyon,Video Konferans" : "Projeksiyon"),
                    new SqlParameter("@IsActive", i % 7 != 0)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }

        for (var i = 1; i <= 35; i++)
        {
            var start = DateTime.UtcNow.Date.AddDays(i % 14 - 3).AddHours(9 + i % 6);
            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (
                    SELECT 1 FROM [dbo].[reservations]
                    WHERE title = @Title AND start_time = @Start)
                BEGIN
                    INSERT INTO [dbo].[reservations]
                    (
                        company_id, meeting_room_id, room_name,
                        organizer_user_id, requester_user_id, requester_name,
                        title, description, start_time, end_time, status,
                        created_at, updated_at
                    )
                    SELECT
                        mr.company_id, mr.meeting_room_id, mr.name,
                        @OrganizerUserId, @OrganizerUserId, N'Rezervasyon Sorumlusu',
                        @Title, @Description, @Start, @End, @Status,
                        SYSUTCDATETIME(), SYSUTCDATETIME()
                    FROM [dbo].[meeting_rooms] AS mr
                    WHERE mr.code = @RoomCode;
                END
                """,
                parameters:
                [
                    new SqlParameter("@Title", $"Toplantı #{i}"),
                    new SqlParameter("@Description", "Haftalık ekip senkronizasyon toplantısı"),
                    new SqlParameter("@Start", start),
                    new SqlParameter("@End", start.AddHours(1)),
                    new SqlParameter("@Status", i % 5 == 0 ? "Cancelled" : i % 3 == 0 ? "Completed" : "Confirmed"),
                    new SqlParameter("@RoomCode", $"ODA-{(i % 18 + 1):D3}"),
                    new SqlParameter("@OrganizerUserId", organizerUserId)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);
        }
    }

    private async Task SeedWeeklyMenusAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int timeout,
        int createdBy,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        for (var week = 0; week < 10; week++)
        {
            var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
            var periodStart = now.Date.AddDays(-daysFromMonday + week * 7);
            var menuCode = $"W{periodStart:yyyyMM}{week + 1}";

            await _executor.ExecuteNonQueryAsync(
                connection,
                """
                IF NOT EXISTS (SELECT 1 FROM [dbo].[weekly_menus] WHERE menu_code = @MenuCode AND company_id = @CompanyId)
                BEGIN
                    INSERT INTO [dbo].[weekly_menus]
                    (
                        company_id, menu_code, year, month, week_of_month,
                        period_start_date, period_end_date, title,
                        is_published, published_at, is_active, created_by,
                        created_at, updated_at
                    )
                    VALUES
                    (
                        @CompanyId, @MenuCode, @Year, @Month, @WeekOfMonth,
                        @PeriodStart, @PeriodEnd, @Title,
                        @IsPublished, CASE WHEN @IsPublished = 1 THEN SYSUTCDATETIME() ELSE NULL END,
                        1, @CreatedBy, SYSUTCDATETIME(), SYSUTCDATETIME()
                    );
                END
                """,
                parameters:
                [
                    new SqlParameter("@CompanyId", PrimaryCompanyId),
                    new SqlParameter("@MenuCode", menuCode),
                    new SqlParameter("@Year", periodStart.Year),
                    new SqlParameter("@Month", periodStart.Month),
                    new SqlParameter("@WeekOfMonth", week % 4 + 1),
                    new SqlParameter("@PeriodStart", periodStart),
                    new SqlParameter("@PeriodEnd", periodStart.AddDays(4)),
                    new SqlParameter("@Title", $"{periodStart:MMMM yyyy} Hafta {week + 1} Menüsü"),
                    new SqlParameter("@IsPublished", week < 8),
                    new SqlParameter("@CreatedBy", createdBy)
                ],
                transaction: transaction,
                commandTimeoutSeconds: timeout,
                cancellationToken: cancellationToken);

            for (var day = 0; day < 5; day++)
            {
                var menuDate = periodStart.AddDays(day);
                for (var item = 0; item < 4; item++)
                {
                    await _executor.ExecuteNonQueryAsync(
                        connection,
                        """
                        IF NOT EXISTS (
                            SELECT 1
                            FROM [dbo].[weekly_menu_items] AS wmi
                            INNER JOIN [dbo].[weekly_menus] AS wm ON wm.menu_id = wmi.menu_id
                            WHERE wm.menu_code = @MenuCode
                              AND wmi.menu_date = @MenuDate
                              AND wmi.sort_order = @SortOrder)
                        BEGIN
                            INSERT INTO [dbo].[weekly_menu_items]
                            (
                                menu_id, menu_date, dish_category_id, dish_id,
                                sort_order, notes, is_active, created_at, updated_at
                            )
                            SELECT
                                wm.menu_id, @MenuDate, dc.dish_category_id, d.dish_id,
                                @SortOrder, NULL, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
                            FROM [dbo].[weekly_menus] AS wm
                            CROSS JOIN (
                                SELECT dish_category_id
                                FROM [dbo].[dish_categories]
                                ORDER BY sort_order
                                OFFSET @CategoryOffset ROWS FETCH NEXT 1 ROWS ONLY
                            ) AS dc
                            CROSS JOIN (
                                SELECT dish_id
                                FROM [dbo].[dishes]
                                WHERE is_active = 1
                                ORDER BY dish_id
                                OFFSET @DishOffset ROWS FETCH NEXT 1 ROWS ONLY
                            ) AS d
                            WHERE wm.menu_code = @MenuCode AND wm.company_id = @CompanyId;
                        END
                        """,
                        parameters:
                        [
                            new SqlParameter("@MenuCode", menuCode),
                            new SqlParameter("@CompanyId", PrimaryCompanyId),
                            new SqlParameter("@MenuDate", menuDate),
                            new SqlParameter("@SortOrder", item),
                            new SqlParameter("@CategoryOffset", item),
                            new SqlParameter("@DishOffset", week * 4 + day + item)
                        ],
                        transaction: transaction,
                        commandTimeoutSeconds: timeout,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
