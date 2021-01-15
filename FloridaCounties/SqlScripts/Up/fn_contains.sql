CREATE FUNCTION dbo.fnGetCountyFor(
	@latitude AS NUMERIC(18, 14),
	@longitude AS NUMERIC(18, 14)) RETURNS TABLE
RETURN 
	SELECT * 
	FROM dbo.tblCounties AS C
	WHERE C.Shape.STContains(geography::STPointFromText(
			CONCAT('POINT(', @longitude, ' ', @latitude, ')'), 4326
		)) = 1;
GO

CREATE FUNCTION dbo.fnGetCityFor(
	@latitude NUMERIC(18, 14),
	@longitude NUMERIC(18, 14)) RETURNS TABLE
RETURN
	SELECT *
	FROM dbo.tblCities AS C
	WHERE C.Shape.MakeValid().STContains(geography::STPointFromText(
			CONCAT('POINT(', @longitude, ' ', @latitude, ')'), 4326
		)) = 1;
GO