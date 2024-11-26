-- Create Webhook Security Tables

-- Clients table to store webhook consumers
CREATE TABLE Clients (
    ClientId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClientName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Webhook tokens table
CREATE TABLE WebhookTokens (
    TokenId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ClientId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(512) NOT NULL,  -- Stores hashed token
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastUsedAt DATETIME2 NULL,
    CONSTRAINT FK_WebhookTokens_Clients FOREIGN KEY (ClientId) REFERENCES Clients(ClientId)
);

-- Token usage tracking for rate limiting and audit
CREATE TABLE TokenUsageLog (
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    TokenId UNIQUEIDENTIFIER NOT NULL,
    UsedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(45) NULL,
    EndpointPath NVARCHAR(256) NOT NULL,
    IsSuccessful BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CONSTRAINT FK_TokenUsageLog_WebhookTokens FOREIGN KEY (TokenId) REFERENCES WebhookTokens(TokenId)
);

-- Create indexes for better performance
CREATE INDEX IX_WebhookTokens_ClientId ON WebhookTokens(ClientId);
CREATE INDEX IX_WebhookTokens_TokenHash ON WebhookTokens(TokenHash);
CREATE INDEX IX_TokenUsageLog_TokenId ON TokenUsageLog(TokenId);
CREATE INDEX IX_TokenUsageLog_UsedAt ON TokenUsageLog(UsedAt);

-- Create stored procedures for token management
GO

CREATE PROCEDURE sp_CreateWebhookToken
    @ClientId UNIQUEIDENTIFIER,
    @TokenHash NVARCHAR(512),
    @ExpiresAt DATETIME2
AS
BEGIN
    INSERT INTO WebhookTokens (ClientId, TokenHash, ExpiresAt)
    VALUES (@ClientId, @TokenHash, @ExpiresAt);
    
    SELECT SCOPE_IDENTITY() AS TokenId;
END
GO

CREATE PROCEDURE sp_ValidateToken
    @TokenHash NVARCHAR(512)
AS
BEGIN
    SELECT 
        wt.TokenId,
        wt.ClientId,
        wt.ExpiresAt,
        wt.IsRevoked,
        c.IsActive AS ClientIsActive
    FROM WebhookTokens wt
    INNER JOIN Clients c ON wt.ClientId = c.ClientId
    WHERE wt.TokenHash = @TokenHash
END
GO

CREATE PROCEDURE sp_RevokeToken
    @TokenId UNIQUEIDENTIFIER
AS
BEGIN
    UPDATE WebhookTokens
    SET IsRevoked = 1,
        LastModifiedAt = GETUTCDATE()
    WHERE TokenId = @TokenId
END
