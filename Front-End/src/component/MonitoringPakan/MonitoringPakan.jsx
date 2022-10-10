import React, { useEffect, useState } from "react";
import ItemInfo from "../ItemInfo/ItemInfo";
import ProgressBar from "../ProgressBar/ProgressBar";
import "./pakanStyles.css";
import LinkHome from "../LinkHome/LinkHome";
import useAxiosJwt from "../../hooks/useAxiosJwt";
import checkLogin from "../../utils/checkLogin";
import { useNavigate } from "react-router-dom";
import ButtonLogout from "../ButtonLogout/ButtonLogout";
import axios from "axios";
import { BASE_URL } from "../../utils/baseUrl";
import timeToString from "../../utils/timeToString";

export default function MonitroingPakan() {
  const [sisaPakan, setSisaPakan] = useState(0);
  const api = useAxiosJwt();
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const [fixDataPakan, setFixDataPakan] = useState(null);

  function parseTime(waktu) {
    return timeToString(waktu, "eeeeeeee, dd MMM yyyy HH:mm:ss");
  }

  async function getDataPakan() {
    const res = await api.get("/feed/ShortRecords");
    return res.data;
  }

  const dataPakanDefault = {
    items: [
      "Jam Pemberian Pakan Ikan",
      "Penggunaan Terakhir Pakan Ikan",
      "Rekap Terakhir Data Pakan",
    ],
    link: {
      title: "Beri Pakan Ikan",
      type: "button",
      action: async function (e) {
        console.log("Sedang memberi pakan ikan");
        e.currentTarget.textContent = "Sedang Memberi Pakan...";
        e.currentTarget.classList.add("btn-wait");
        let intervalPakan = setInterval(async () => {
          const response = await axios.get(`${BASE_URL}/feed/CheckTimeToFeed`);
          console.log(response.data);
          if (response.data === 0) {
            const btnPakan =
              document.querySelector(".btn-wait") ||
              document.querySelector(".btn-goal");
            // update data pakan
            const newData = await getDataPakan();
            dataPakanDefault.values = [
              ["08:00", "16:30"],
              [
                `${newData.info.beratPakan} gr`,
                parseTime(newData.info.waktuPakan),
              ],
              [
                `Hari Ini: ${(newData.infoRekap.hari / 1000).toFixed(2)} kg`,
                `Seminggu: ${(newData.infoRekap.minggu / 1000).toFixed(2)} kg`,
                `Sebulan: ${(newData.infoRekap.bulan / 1000).toFixed(2)} kg`,
              ],
            ];
            setFixDataPakan(dataPakanDefault);
            setSisaPakan(newData.info.sisaPakan);
            // pesan berhasil
            btnPakan.classList.remove("btn-wait");
            btnPakan.classList.add("btn-goal");
            btnPakan.textContent = "Berhasil Memberi Pakan";
            // delay 3 detik
            await new Promise((resolve) => setTimeout(resolve, 3000));
            // set button ke defaul
            btnPakan.classList.remove("btn-goal");
            btnPakan.textContent = "Beri Pakan Ikan";
            // clear interval cek kondisi makan
            clearInterval(intervalPakan);
          }
        }, [2500]);
        const resAxios = await api.get("/feed/FeedTheFish");
        console.log(resAxios.data);
      },
    },
  };

  const [dataPakan, setDataPakan] = useState(dataPakanDefault);

  useEffect(() => {
    (async () => {
      // cek pakah sudah login apa belum
      const isLogin = await checkLogin();
      // jika sudah login ambil data di server
      if (isLogin) {
        setLoading(false);
        if (fixDataPakan) {
          setDataPakan(fixDataPakan);
        } else {
          fetchFeedRecords();
        }
      } else {
        // jika belum login redirect ke halaman login
        navigate("/");
      }
    })();
    // eslint-disable-next-line
  }, [fixDataPakan]);

  // sebuah fungsi untuk mengambil data pakan terbaru dari server
  const fetchFeedRecords = async () => {
    try {
      const response = await api.get("/feed/ShortRecords");
      const data = response.data;
      setSisaPakan(data.info.sisaPakan);
      dataPakanDefault.values = [
        ["08:00", "16:30"],
        [`${data.info.beratPakan} gr`, parseTime(data.info.waktuPakan)],
        [
          `Hari Ini: ${(data.infoRekap.hari / 1000).toFixed(2)} kg`,
          `Seminggu: ${(data.infoRekap.minggu / 1000).toFixed(2)} kg`,
          `Sebulan: ${(data.infoRekap.bulan / 1000).toFixed(2)} kg`,
        ],
      ];
      setDataPakan(dataPakanDefault);
    } catch (error) {
      console.log("Error maseh");
    }
  };
  // end function

  return loading ? (
    <div></div>
  ) : (
    <div className="mt-30">
      <ButtonLogout />
      <LinkHome />
      <div className="monitoring-pakan">
        <h5>Informasi Monitoring Pakan Ikan</h5>
        <div className="body-monitoring">
          <ProgressBar value={sisaPakan} title={"Sisa Pakan"} />
          <ItemInfo data={dataPakan} titleInfo={"Info Pemberian pakan"} />
        </div>
      </div>
    </div>
  );
}
