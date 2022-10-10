using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Web_ASP.Models;

namespace Web_ASP
{

    [Route("api/[controller]")]
    [ApiController]
    public class FeedController : ControllerBase
    {
        private DbPrimus dbPrimus = new DbPrimus();
        private TimeModel timeHelper = new TimeModel();
        private string _token;

        private IConfiguration _config;

        public FeedController(IConfiguration config)
        {
            _config = config;
            _token = _config["TokenAlat:Key"];
        }

        // method pakan 
        // mengambil data pemberian pakan terakhir
        [HttpGet("ShortRecords")]
        [Authorize]
        public IActionResult ShortRecords()
        {
            string records = JsonConvert.SerializeObject(dbPrimus.getFeedShortRecords());

            return Content(records);
        }

        // method untuk melakukan update data setelah memberi pakan 
        // digunakan di microcontroller
        [HttpGet("afterFeed/{kondisi}/{berat_pakan}/{sisa_pakan}/{token}")]
        public IActionResult afterFeed(int kondisi, float berat_pakan, int sisa_pakan, string token)
        {
            string waktu_pakan = timeHelper.GetTimeNow("yyyy-MM-dd HH:mm:ss");
            int update = 0;
            int insert = 0;
            // cek string token
            if (_token == token)
            {
                update = dbPrimus.updatePakan(kondisi, berat_pakan, sisa_pakan, waktu_pakan);
                insert = dbPrimus.addFeedRecord(berat_pakan, waktu_pakan);
            }
            // cek query update
            if (update <= 0 && insert <= 0) return Content("{'message' : 'Gagal Menambahkan Data!'}");
            // kirim pesan berhasil jika berhasil
            return Content("{'message': 'Berhasil Memberi Pakan'}");
        }

        // method untuk memberi pakan 
        [HttpGet("FeedTheFish")]
        [Authorize]
        public IActionResult FeedTheFish()
        {
            string waktu_pakan = timeHelper.GetTimeNow("MM/dd/yyyy HH:mm:ss");
            // Update Pakan
            int cek = 0;
            cek = dbPrimus.updateKondisi(1);
            string status = "";
            status = cek == 1 ? "Berhasil" : "Gagal";
            return new ObjectResult(new
            {
                Message = status,
                Waktu = waktu_pakan
            });
        }

        // method untuk cek kondisi pakan
        [HttpGet("CheckTimeToFeed")]
        public IActionResult CheckTimeToFeed()
        {
            return Content(dbPrimus.feedCheck());
        }
        // end method pakan

        // method feed record
        // method untuk mengambil record pakan
        public string feedRecordsPerDay(int day)
        {
            string record = "";
            string sekarang = timeHelper.GetTimeNow("yyyy-MM-dd HH:mm:ss");
            string kemarin = timeHelper.GetSubstractTimeNow(day, "yyyy-MM-dd HH:mm:ss");
            record = JsonConvert.SerializeObject(dbPrimus.getFeedRecordsBetween(kemarin, sekarang));
            return record;
        }

        // mengambil data pakan selama sehari terakhir
        [HttpGet("feedRecordsLastDay")]
        [Authorize]
        public IActionResult feedRecordsLastDay()
        {
            return Content(feedRecordsPerDay(1));
        }

        // mengambil data pakan sebulan terakhir
        [HttpGet("feedRecordsLastMonth")]
        [Authorize]
        public IActionResult feedRecordsLastMonth()
        {
            return Content(feedRecordsPerDay(30));
        }

        // Mengambil data pakan setahun terakhir
        [HttpGet("feedRecordsLastYear")]
        [Authorize]
        public IActionResult feedRecordsLastYear()
        {
            return Content(feedRecordsPerDay(365));
        }
        // end method feed records
    }
}