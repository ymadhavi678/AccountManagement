USE [Learning]
GO
/****** Object:  Table [dbo].[Accounts]    Script Date: 30-08-2024 10:47:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[Id] [bigint] NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transactions]    Script Date: 30-08-2024 10:47:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transactions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FromAccountId] [bigint] NOT NULL,
	[ToAccountId] [bigint] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Timestamp] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (getutcdate()) FOR [Timestamp]
GO
ALTER TABLE [dbo].[Transactions]  WITH CHECK ADD FOREIGN KEY([FromAccountId])
REFERENCES [dbo].[Accounts] ([Id])
GO
ALTER TABLE [dbo].[Transactions]  WITH CHECK ADD FOREIGN KEY([ToAccountId])
REFERENCES [dbo].[Accounts] ([Id])
GO
/****** Object:  StoredProcedure [dbo].[TransferMoney]    Script Date: 30-08-2024 10:47:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[TransferMoney]
    @FromAccountId BIGINT,
    @ToAccountId BIGINT,
    @Amount DECIMAL(18, 2),
    @Description NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Check if accounts exist
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Id = @FromAccountId)
            THROW 50000, 'From account not found.', 1;
        
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Id = @ToAccountId)
            THROW 50000, 'To account not found.', 1;
        
        -- Check if the FromAccount has sufficient funds
        DECLARE @CurrentBalance DECIMAL(18, 2);
        
        SELECT @CurrentBalance = Balance
        FROM Accounts
        WHERE Id = @FromAccountId;
        
        IF @CurrentBalance < @Amount
            THROW 50000, 'Insufficient funds.', 1;
        
        -- Begin transaction
        BEGIN TRANSACTION;
        
        -- Deduct amount from FromAccount
        UPDATE Accounts
        SET Balance = Balance - @Amount
        WHERE Id = @FromAccountId;

        -- Add amount to ToAccount
        UPDATE Accounts
        SET Balance = Balance + @Amount
        WHERE Id = @ToAccountId;

        -- Insert transaction record
        INSERT INTO Transactions (FromAccountId, ToAccountId, Amount, Timestamp)
        VALUES (@FromAccountId, @ToAccountId, @Amount, GETUTCDATE());

        -- Commit transaction
        COMMIT TRANSACTION;
        
        SELECT 'Transfer successful' AS Message;
    END TRY
    BEGIN CATCH
        -- Rollback transaction in case of error
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Rethrow error
        THROW;
    END CATCH;
END;
GO
