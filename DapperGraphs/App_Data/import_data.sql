/****** Script do comando SelectTopNRows de SSMS  ******/
SELECT *
  FROM [TransformacaoCodingCraft].[dbo].[energy_use_per_person$]

insert into [DapperGraphs]..[Countries] (CountryId, Name)
select NEWID(), country from [TransformacaoCodingCraft].[dbo].[energy_use_per_person$]

begin
	declare @year int;
	select @year = 1960

	while @year <= 2015
	begin
		exec('insert into [DapperGraphs]..[EnergyUsageDatas] (EnergyUsageDataId, CountryId, "Year", Value) ' +
		'select NEWID(), c.CountryId, ' + @year + ', isnull(t.[' + @year + '], 0) ' +
		'from [DapperGraphs]..[Countries] c ' +
		'inner join [TransformacaoCodingCraft].[dbo].[energy_use_per_person$] t on (t.country = c.Name) ');
		select @year = @year + 1
	end
end