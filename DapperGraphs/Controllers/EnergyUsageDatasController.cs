using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DapperGraphs.Models;
using Dapper;
using DapperGraphs.ViewModels;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace DapperGraphs.Controllers
{
    [Authorize]
    public class EnergyUsageDatasController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// GET: gera relatório básico de uso de energia per capita anul de cada páis (todo período)
        /// </summary>
        /// <returns>Retorna uma lista de <see cref="EnergyUsageData"/> para exibição em listagem.</returns>
        public async Task<ActionResult> Index()
        {
            string sql = @"select e.EnergyUsageDataId, e.CountryId, e.Year, e.Value, c.CountryId, c.Name
                        from EnergyUsageDatas e
                        join countries c on c.CountryId = e.CountryId
                        order by c.Name";

            using (db)
            {
                var energyUsageDatas = await db.Database.Connection.QueryAsync<EnergyUsageData, Country, EnergyUsageData>(sql,
                    (energyUsageData, country) =>
                    {
                        energyUsageData.Country = country;
                        return energyUsageData;
                    },
                    splitOn: "Value,CountryId"
                );

                return View(energyUsageDatas);
            }
        }

        /// <summary>
        /// GET: Action para específicar parâmetros de consulta de Relatório análitico por período de ano de determinado país.
        /// Ao inicilizar a lista de países é carregada do BD para seleção do usuário.
        /// </summary>
        /// <returns>View de RelatorioAnalitico com os campos a serem parametrizados pelo usuários.</returns>
        public ActionResult RelatorioAnalitico()
        {
            using (db)
            {
                var paises = db.Countries.OrderBy(c => c.Name).ToList();
                List<SelectListItem> listItemPaises = paises.Select(x => new SelectListItem() { Value = x.CountryId.ToString(), Text = x.Name.ToString() }).ToList();
                listItemPaises.Insert(0, new SelectListItem { Value = string.Empty, Text = "Escolher..." });
                ViewBag.Paises = listItemPaises;
                return View();
            }
        }

        /// <summary>
        /// GET: gerar o relatório parametrizado em <see cref="RelatorioAnalitico"/>.
        /// </summary>
        /// <param name="anoInicial">Ano inicial do período a ser consultado.</param>
        /// <param name="anoFinal">Ano final do período a ser consultado.</param>
        /// <param name="paises">Id (Guid) do país a ser consultado.</param>
        /// <returns>Retorna para a view GerarRelatorioAnalitico uma lista de <see cref="EnergyUsageData"/> para geração do relatório.</returns>
        public async Task<ActionResult> GerarRelatorioAnalitico(short anoInicial, short anoFinal, Guid paises)
        {
            string sql = @"select e.EnergyUsageDataId, e.CountryId, e.Year, e.Value, c.CountryId, c.Name
                        from EnergyUsageDatas e
                        join countries c on c.CountryId = e.CountryId
                        where c.CountryId = @IdPais
                        and e.Year >= @IdAnoInicio and e.Year <= @IdAnoFim
                        and e.Value > 0
                        order by e.Year";

            using (db)
            {
                //Dapper:
                var energyUsageDatas = await db.Database.Connection.QueryAsync<EnergyUsageData, Country, EnergyUsageData>(sql,
                    (energyUsageData, country) =>
                    {
                        energyUsageData.Country = country;
                        return energyUsageData;
                    },
                    splitOn: "Value,CountryId",
                    param: new
                    {
                        IdPais = paises.ToString(),
                        IdAnoInicio = anoInicial,
                        IdAnoFim = anoFinal
                    }
                );

                return View(energyUsageDatas);
            }
        }

        /// <summary>
        /// View com campos para parametrizar relatório de métricas.
        /// </summary>
        /// <returns>Retorna a view com campos de parametrização do relatório.</returns>
        public ActionResult RelatorioAnaliticoMetricas()
        {
            return View();
        }

        /// <summary>
        /// Gerar relatório de métricas parametrizado em <see cref="RelatorioAnaliticoMetricas"/>.
        /// </summary>
        /// <param name="anoInicial">Ano inicial do período a ser consultado.</param>
        /// <param name="anoFinal">Ano final do período a ser consultado.</param>
        /// <returns></returns>
        public async Task<ActionResult> GerarRelatorioAnaliticoMetricas(short anoInicial, short anoFinal)
        {
            string sql = @"select c.Name as NomePais, AVG(e.Value) as media, STDEVP(e.Value) as DesvioPadrao,  MIN(e.Value) as Minimo, MAX(e.Value) as Maximo
                            from EnergyUsageDatas e
                            join countries c on c.CountryId = e.CountryId
                            where e.Year >= @IdAnoInicio and e.Year <= @IdAnoFim
                            and e.Value > 0
                            group by c.Name
                            order by c.Name";
            
            using (db)
            {
                var relatorioMetricas = await db.Database.Connection.QueryAsync<RelatorioMetricasViewModels>(sql,
                    param: new
                    {
                        IdAnoInicio = anoInicial,
                        IdAnoFim = anoFinal
                    }
                );

                return View(relatorioMetricas);
            }
        }

        /// <summary>
        /// Gerar gráfico de metricas com Média, Desvio Padrão, Mínimo e Máximo de 4 países aleatórios.
        /// Após carregar o gráfico com os países aleatórios o usuários pode configurar os filtros e período como preferir.
        /// </summary>
        /// <returns>Retorna a view com o gráfico com os países selecionados aleatóriamente.</returns>
        public ActionResult GerarGraficoMetricas()
        {
            MetricChartParameterViewModel metricChartData = new MetricChartParameterViewModel();
            metricChartData.AnoInicial = 1990;
            metricChartData.AnoFinal = 2005;

            using (db)
            {
                var paises = db.Countries.ToList();

                metricChartData.ListaPaises = paises.OrderBy(o => o.Name).Select(x =>
                    new SelectListItem()
                    {
                        Value = x.CountryId.ToString(),
                        Text = x.Name
                    });

                string sql = @"select TOP(4)
                            c.Name as NomePais, AVG(e.Value) as media, STDEVP(e.Value) as DesvioPadrao,  MIN(e.Value) as Minimo, MAX(e.Value) as Maximo
                            from EnergyUsageDatas e
                            join countries c on c.CountryId = e.CountryId
                            where e.Year >= @IdAnoInicio and e.Year <= @IdAnoFim
                            and c.CountryId in @listaIdPaises
                            and e.Value > 0
                            group by c.Name
                            order by NEWID()";

                ViewBag.StartChart = db.Database.Connection.Query<RelatorioMetricasViewModels>(sql,
                    param: new
                    {
                        IdAnoInicio = metricChartData.AnoInicial,
                        IdAnoFim = metricChartData.AnoFinal,
                        listaIdPaises = paises.Select(s => s.CountryId).ToArray()
                    }
                );

                return View(metricChartData);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anoInicial"></param>
        /// <param name="anoFinal"></param>
        /// <param name="listaIdPaises"></param>
        /// <returns></returns>
        /// <remarks>https://jsfiddle.net/Wurthmann/hfjutm57/10/</remarks>
        [HttpPost]
        public JsonResult GerarDadosGraficoMetricas(MetricChartParameterViewModel paramsMetricChart)
        {
            string sql = @"select c.Name as NomePais, AVG(e.Value) as media, STDEVP(e.Value) as DesvioPadrao,  MIN(e.Value) as Minimo, MAX(e.Value) as Maximo
                            from EnergyUsageDatas e
                            join countries c on c.CountryId = e.CountryId
                            where e.Year >= @IdAnoInicio and e.Year <= @IdAnoFim
                            and c.CountryId in @listaIdPaises
                            and e.Value > 0
                            group by c.Name
                            order by c.Name";

            using (db)
            {
                var relatorioMetricas = db.Database.Connection.Query<RelatorioMetricasViewModels>(sql,
                    param: new
                    {
                        IdAnoInicio = paramsMetricChart.AnoInicial,
                        IdAnoFim = paramsMetricChart.AnoFinal,
                        listaIdPaises = paramsMetricChart.ListaIdPaises
                    }
                );

                return Json(relatorioMetricas);
            }
        }

        /// <summary>
        /// Gráfico de colunas com o TOP 10 países com maior valor de energia per capita durante todo período.
        /// </summary>
        /// <returns>Retorna a view com uma gráfico de coluna populado por <see cref="GerarDadosRelatorioTop10"/></returns>
        public ActionResult GerarGraficoTop10()
        {
            return View();
        }

        /// <summary>
        /// POST: Método para gerar JSON com os dados do gráfico de barras.
        /// </summary>
        /// <returns>Retorna o JSON com os dados dos países TOP 10.</returns>
        [HttpPost]
        public JsonResult GerarDadosRelatorioTop10()
        {
            using (db)
            {
                var result = db.Database.Connection.Query<ColumnGraphViewModel>(@"select top (10) 
                c.Name Pais, sum(e.Value) Valor
                from Countries c
                join EnergyUsageDatas e on e.CountryId = c.CountryId
                group by c.Name
                order by Valor desc");

                return Json(result);
            }
        }

        /// <summary>
        /// Gráfico de linhas com o TOP 10 países e seus valores de uso de energia per capita.
        /// </summary>
        /// <returns>Retorna a view com uma gráfico de linhas populado por <see cref="GerarDadosRelatorioLinhaTop10"/></returns>
        public ActionResult GerarGraficoLinhaTop10()
        {
            return View();
        }

        /// <summary>
        /// POST: Gerar os dados para montar o gráfico de linha de países Top 10 em uso de energia per capita.
        /// </summary>
        /// <returns>Retorna JSON com os dados para montar gráfico de linhas.</returns>
        [HttpPost]
        public JsonResult GerarDadosRelatorioLinhaTop10()
        {
            List<LineGraphJSONViewModel> result = new List<LineGraphJSONViewModel>();
            IEnumerable<LineGraphDataViewModel> listaValores;

            using (db)
            {
                listaValores = db.Database.Connection.Query<LineGraphDataViewModel>(
                @"select c.CountryId, c.Name, e.Year, e.Value 
                from EnergyUsageDatas e
                join Countries c on c.CountryId = e.CountryId
                and c.CountryId in (
	                select CountryId from (
		                select top(10)
		                c_.CountryId, c_.Name Pais, sum(e_.Value) Valor
		                from Countries c_
		                join EnergyUsageDatas e_ on e_.CountryId = c_.CountryId
		                group by c_.Name, c_.CountryId
		                order by Valor desc
	                ) Top10
                )
                order by c.Name, e.Year");
            }

            var listaPaises = listaValores.Select(p => p.CountryId).Distinct();

            LineGraphJSONViewModel jsonData = new LineGraphJSONViewModel();
            jsonData.ListDataPerYear = new List<LineDatumJSONViewModel>();
            //Atribui o nome do pais
            jsonData.Countrys = listaValores.Select(c => c.Name).Distinct().ToList();

            //Seleciona o período completo de anos
            var years = listaValores.Select(y => y.Year).Distinct().OrderBy(y => y).ToList();

            //Adiciona os valores de cada ano a lista
            foreach(var year in years)
            {
                LineDatumJSONViewModel auxDatas = new LineDatumJSONViewModel();
                auxDatas.Year = year;
                auxDatas.ListValues = listaValores.Where(y => y.Year == year).OrderBy(c => c.Name).Select(v => v.Value).ToList();
                jsonData.ListDataPerYear.Add(auxDatas);
            }

            result.Add(jsonData);

            return Json(result);
        }

        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnergyUsageData energyUsageData = await db.EnergyUsageDatas.FindAsync(id);
            if (energyUsageData == null)
            {
                return HttpNotFound();
            }
            return View(energyUsageData);
        }

        // GET: EnergyUsageDatas/Create
        public ActionResult Create()
        {
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "Name");
            return View();
        }

        // POST: EnergyUsageDatas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "EnergyUsageDataId,Year,Value,CountryId")] EnergyUsageData energyUsageData)
        {
            if (ModelState.IsValid)
            {
                energyUsageData.EnergyUsageDataId = Guid.NewGuid();
                db.EnergyUsageDatas.Add(energyUsageData);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "Name", energyUsageData.CountryId);
            return View(energyUsageData);
        }

        // GET: EnergyUsageDatas/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnergyUsageData energyUsageData = await db.EnergyUsageDatas.FindAsync(id);
            if (energyUsageData == null)
            {
                return HttpNotFound();
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "Name", energyUsageData.CountryId);
            return View(energyUsageData);
        }

        // POST: EnergyUsageDatas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "EnergyUsageDataId,Year,Value,CountryId")] EnergyUsageData energyUsageData)
        {
            if (ModelState.IsValid)
            {
                db.Entry(energyUsageData).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CountryId = new SelectList(db.Countries, "CountryId", "Name", energyUsageData.CountryId);
            return View(energyUsageData);
        }

        // GET: EnergyUsageDatas/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnergyUsageData energyUsageData = await db.EnergyUsageDatas.FindAsync(id);
            if (energyUsageData == null)
            {
                return HttpNotFound();
            }
            return View(energyUsageData);
        }

        // POST: EnergyUsageDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            EnergyUsageData energyUsageData = await db.EnergyUsageDatas.FindAsync(id);
            db.EnergyUsageDatas.Remove(energyUsageData);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
