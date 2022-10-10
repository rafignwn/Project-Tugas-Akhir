using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Web_ASP.Models;

namespace Web_ASP
{

    [Route("api/[controller]")]
    [ApiController]
    public class PHController : ControllerBase
    {
        private DbPrimus db_primus = new DbPrimus();

        private TimeModel timeHelper = new TimeModel();
        private string _token;

        private IConfiguration _config;

        public PHController(IConfiguration config)
        {
            _config = config;
            _token = _config["TokenAlat:Key"];
        }

        [HttpGet("GetPhValue")]
        [Authorize]
        public IActionResult GetPhValue()
        {
            string api = "";
            api = JsonConvert.SerializeObject(db_primus.getAllData());
            return Content(api);
        }

        // [AllowAnonymous]
        [HttpGet("CurrentpHValue")]
        [Authorize]
        public IActionResult CurrentpHValue()
        {
            return new ObjectResult(db_primus.getCurrentData());
        }


        [HttpGet("AddPhValue/{nilai_ph}/{token}")]
        public IActionResult AddPhValue(float nilai_ph, string token)
        {
            string timeNow = timeHelper.GetTimeNow("yyyy-MM-dd HH:mm:ss");
            string level_ph = GetPhLevel(nilai_ph);
            string kondisi = "";
            int insert = 0;
            // Cek Kondisi pH
            if (nilai_ph >= 7.5 && nilai_ph <= 8.5)
            {
                kondisi = "BAIK";
            }
            else
            {
                kondisi = "KURANG BAIK";
            }
            // Cek jumlah data 
            if (db_primus.getRowNumbers() > 10000) db_primus.deleteTopOneData();
            // insert to database
            if (_token == token) insert = db_primus.addData(nilai_ph, level_ph, kondisi, timeNow);
            if (insert < 0)
            {
                return Content("Gagal menambahkan data!");
            }
            return new ObjectResult(new
            {
                message = "Berhasil menambahkan data pH",
                nilaipH = nilai_ph,
                levelpH = level_ph,
                waktuInput = timeNow
            });
        }

        // method uji coba kirim data ph
        [HttpGet("AddPh/{nilai_ph}")]
        public IActionResult AddPh(float nilai_ph)
        {
            string timeNow = timeHelper.GetTimeNow("yyyy-MM-dd HH:mm:ss");
            string level_ph = GetPhLevel(nilai_ph);
            return new ObjectResult(new
            {
                message = "Berhasil menambahkan data pH",
                nilaipH = nilai_ph,
                levelpH = level_ph,
                waktuInput = timeNow
            });
        }

        // method Record pH
        // megnambil data satu hari terakhir
        [HttpGet("RecordPhValueLastDay")]
        [Authorize]
        public IActionResult RecordPhValueLastDay()
        {
            return Content(RecordPhvaluePerDay(1));
        }

        // megnambil data satu hari terakhir
        [HttpGet("RecordPhValueLastWeek")]
        [Authorize]
        public IActionResult RecordPhValueLastWeek()
        {
            return Content(RecordPhvaluePerDay(7));
        }

        // mengambil data sebulan terakhir
        [HttpGet("RecordPhValueLastMonth")]
        [Authorize]
        public IActionResult RecordPhValueLastMonth()
        {
            return Content(RecordPhvaluePerDay(30));
        }

        // Mengambil data setahun terakhir
        [HttpGet("RecordPhValueLastYear")]
        [Authorize]
        public IActionResult RecordPhValueLastYear()
        {
            return Content(RecordPhvaluePerDay(365));
        }

        // method untuk mengambil data pH dengan parameter hari
        protected string RecordPhvaluePerDay(int day)
        {
            string Record = "";
            string sekarang = timeHelper.GetTimeNow("yyyy-MM-dd HH:mm:ss");
            string kemarin = timeHelper.GetSubstractTimeNow(day, "yyyy-MM-dd HH:mm:ss");
            Record = JsonConvert.SerializeObject(db_primus.getDataPhBetween(kemarin, sekarang));
            return Record;
        }
        // end method Record pH

        public string GetPhLevel(float nilaiPh)
        {
            string result = "";
            // cek level pH
            if (nilaiPh <= 7.4 && nilaiPh >= 6.8)
            {
                result = "NETRAL";
            }
            else if (nilaiPh > 7.4)
            {
                result = "BASA";
            }
            else
            {
                result = "ASAM";
            }
            return result;
        }
    }
}