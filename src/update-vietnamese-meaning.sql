-- Update existing VocabularyWords to have VietnameseMeaning = Meaning if VietnameseMeaning is empty
-- This is a one-time update for existing data

UPDATE VocabularyWords 
SET VietnameseMeaning = Meaning
WHERE VietnameseMeaning IS NULL OR VietnameseMeaning = '';

-- Verify update
SELECT Id, Word, Meaning, VietnameseMeaning 
FROM VocabularyWords 
ORDER BY Id;
