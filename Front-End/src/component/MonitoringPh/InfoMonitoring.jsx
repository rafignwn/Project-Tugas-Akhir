import React, { useEffect, useState } from "react";
import ItemInfo from "../ItemInfo/ItemInfo";
import useAxiosJwt from "../../hooks/useAxiosJwt";
import "./btnResetStyle.css";

const items = {
  items: ["Nilai pH Saat Ini", "Level pH", "Kondisi pH Tambak", "Jumlah Data"],
};

export default function InfoMonitoring() {
  const [dataPh, setDataPh] = useState(items);
  const api = useAxiosJwt();

  useEffect(() => {
    fetchPHRecords();
    // eslint-disable-next-line
  }, []);

  // sebuah fungsi untuk mengambil data pH yang terbaru dari server
  const fetchPHRecords = async () => {
    try {
      const response = await api.get("/ph/CurrentpHValue");
      const data = response.data;
      setDataPh({
        ...items,
        values: [
          [data.nilaiPh],
          [data.levelPh],
          [data.kondisi],
          [data.jumlah_data],
        ],
      });
    } catch (error) {
      console.log("Error maseh");
    }
  };

  async function handlerReset() {
    const yesDelete = window.confirm("Pakah Anda Yakin ?");
    if (yesDelete) {
      const res = await api.get("/ph/resetData");
      const pesan = await res.json();
      console.log(pesan);
    } else {
      console.log("tidak jadi");
    }
  }
  // end function

  return (
    <div className="info-monitoring">
      <ItemInfo data={dataPh} titleInfo={"Informasi Monitoring"} />
      <button onClick={handlerReset} className="btn-reset">
        Reset Data
      </button>
    </div>
  );
}
