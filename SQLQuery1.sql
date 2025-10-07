UPDATE [dbo].[Table]
SET Image = (
    SELECT BulkColumn 
    FROM OPENROWSET(BULK 'C:\Users\salbi\OneDrive\Рабочий стол\ISREC-master\Image\photo_2025-05-30_00-12-44.jpg', SINGLE_BLOB) AS Image
)
WHERE Id = 23;