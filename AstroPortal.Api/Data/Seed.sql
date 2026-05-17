IF DB_ID('AstroPortalDb') IS NULL
BEGIN
    CREATE DATABASE AstroPortalDb;
END;
GO

USE AstroPortalDb;
GO

IF OBJECT_ID('dbo.BlogPosts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BlogPosts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Slug NVARCHAR(220) NOT NULL UNIQUE,
        Excerpt NVARCHAR(500) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        ImageUrl NVARCHAR(600) NOT NULL,
        IsPublished BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(160) NOT NULL,
        Description NVARCHAR(600) NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Price DECIMAL(10,2) NOT NULL,
        ImageUrl NVARCHAR(600) NOT NULL,
        InStock BIT NOT NULL DEFAULT 1
    );
END;

IF OBJECT_ID('dbo.Testimonials', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Testimonials (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CustomerName NVARCHAR(120) NOT NULL,
        Location NVARCHAR(120) NOT NULL,
        Quote NVARCHAR(800) NOT NULL,
        Rating INT NOT NULL,
        AvatarUrl NVARCHAR(600) NOT NULL
    );
END;

IF OBJECT_ID('dbo.ContactRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(140) NOT NULL,
        Email NVARCHAR(180) NOT NULL,
        Phone NVARCHAR(40) NOT NULL,
        Subject NVARCHAR(180) NOT NULL,
        Message NVARCHAR(1200) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID('dbo.KundaliRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KundaliRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(140) NOT NULL,
        Email NVARCHAR(180) NOT NULL,
        Phone NVARCHAR(40) NOT NULL,
        BirthDate DATE NOT NULL,
        BirthTime TIME NOT NULL,
        BirthPlace NVARCHAR(220) NOT NULL,
        ServiceType NVARCHAR(120) NOT NULL,
        Notes NVARCHAR(1200) NOT NULL,
        Status NVARCHAR(60) NOT NULL DEFAULT 'New',
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;

IF OBJECT_ID('dbo.HoroscopeCaches', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoroscopeCaches (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Sign NVARCHAR(40) NOT NULL,
        HoroscopeDate DATE NOT NULL,
        Content NVARCHAR(1600) NOT NULL,
        Source NVARCHAR(120) NOT NULL,
        FetchedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UX_HoroscopeCaches_Sign_Date UNIQUE (Sign, HoroscopeDate)
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BlogPosts)
BEGIN
    INSERT INTO dbo.BlogPosts (Title, Slug, Excerpt, Content, Category, ImageUrl)
    VALUES
    ('How Daily Horoscopes Help You Plan Mindfully', 'daily-horoscopes-mindful-planning', 'Learn how zodiac insights can become a gentle ritual for intention-setting and better choices.', 'Daily horoscopes are best used as reflective prompts. Read your sign, note the theme, and choose one practical action for relationships, career, or self-care.', 'Horoscope', 'https://images.unsplash.com/photo-1532968961962-8a0cb3a2d4f5?auto=format&fit=crop&w=1200&q=80'),
    ('Kundali Basics: Birth Time, Place and Planetary Houses', 'kundali-basics-birth-time-place-houses', 'A beginner-friendly guide to the key details needed for accurate kundali services.', 'A kundali maps the sky at your exact birth moment. Accurate date, time, and place help astrologers interpret planetary houses with more confidence.', 'Kundali', 'https://images.unsplash.com/photo-1515942661900-94b3d1972591?auto=format&fit=crop&w=1200&q=80'),
    ('Choosing Religious Items for a Peaceful Home Altar', 'religious-items-peaceful-home-altar', 'Simple tips for selecting idols, malas, incense and diya sets for your sacred corner.', 'Choose altar items that are meaningful, easy to maintain, and aligned with your daily practice. Keep the space clean, bright, and intentional.', 'Spiritual Living', 'https://images.unsplash.com/photo-1609607847926-da4702f01fef?auto=format&fit=crop&w=1200&q=80');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    INSERT INTO dbo.Products (Name, Description, Category, Price, ImageUrl)
    VALUES
    ('Rudraksha Mala', 'A traditional meditation mala for mantra chanting and daily grounding.', 'Mala', 1499, 'https://images.unsplash.com/photo-1602173574767-37ac01994b2a?auto=format&fit=crop&w=900&q=80'),
    ('Brass Diya Set', 'Elegant brass diyas for puja rituals, festivals and home altar lighting.', 'Puja Essentials', 899, 'https://images.unsplash.com/photo-1605368380945-7472b7fb0697?auto=format&fit=crop&w=900&q=80'),
    ('Sandalwood Incense', 'Calming sandalwood incense sticks for meditation and peaceful spaces.', 'Incense', 349, 'https://images.unsplash.com/photo-1602928321679-560bb453f190?auto=format&fit=crop&w=900&q=80'),
    ('Crystal Healing Kit', 'Curated crystals for focus, clarity and positive energy practices.', 'Crystals', 2199, 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=900&q=80'),
    ('Puja Thali', 'Complete decorative thali for daily worship and auspicious ceremonies.', 'Puja Essentials', 1299, 'https://images.unsplash.com/photo-1583394838336-acd977736f90?auto=format&fit=crop&w=900&q=80');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Testimonials)
BEGIN
    INSERT INTO dbo.Testimonials (CustomerName, Location, Quote, Rating, AvatarUrl)
    VALUES
    ('Ananya Sharma', 'Delhi', 'The kundali consultation was thoughtful, practical and easy to understand.', 5, 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80'),
    ('Rohit Mehta', 'Mumbai', 'Daily guidance and remedies helped me bring structure to important decisions.', 5, 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80'),
    ('Meera Kapoor', 'Jaipur', 'Beautiful products, quick response, and a very professional astrology experience.', 5, 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80');
END;
GO
