USE [Learning]
GO
/****** Object:  Table [dbo].[Accounts]    Script Date: 02-09-2024 21:59:13 ******/
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
/****** Object:  Table [dbo].[Transactions]    Script Date: 02-09-2024 21:59:13 ******/
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
	[TransactionDescription] [nvarchar](255) NULL,
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
/****** Object:  StoredProcedure [dbo].[TransferMoney]    Script Date: 02-09-2024 21:59:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[TransferMoney]
    @FromAccountId BIGINT,
    @ToAccountId BIGINT,
    @Amount DECIMAL(18, 2),
    @TransactionDescription NVARCHAR(255) = NULL  -- Optional parameter
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Start a transaction
        BEGIN TRANSACTION;

        -- Check if the FromAccount has enough balance
        DECLARE @FromAccountBalance DECIMAL(18, 2);

        SELECT @FromAccountBalance = Balance
        FROM [dbo].[Accounts]
        WHERE Id = @FromAccountId;

        IF @FromAccountBalance IS NULL
        BEGIN
            RAISERROR('FromAccount does not exist.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @FromAccountBalance < @Amount
        BEGIN
            RAISERROR('Insufficient funds in FromAccount.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- Deduct the amount from the FromAccount
        UPDATE [dbo].[Accounts]
        SET Balance = Balance - @Amount
        WHERE Id = @FromAccountId;

        -- Add the amount to the ToAccount
        UPDATE [dbo].[Accounts]
        SET Balance = Balance + @Amount
        WHERE Id = @ToAccountId;

        -- Insert the transaction record with optional TransactionDescription
        INSERT INTO [dbo].[Transactions] (FromAccountId, ToAccountId, Amount, TransactionDescription)
        VALUES (@FromAccountId, @ToAccountId, @Amount, @TransactionDescription);

        -- Commit the transaction
        COMMIT TRANSACTION;

        PRINT 'Transfer successful.';
    END TRY
    BEGIN CATCH
        -- Rollback the transaction if an error occurs
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END

        -- Capture the error information
        DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        -- Raise the error using RAISERROR
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
