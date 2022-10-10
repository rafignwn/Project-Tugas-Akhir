import React, { useEffect, useRef, useState } from "react";
import ToplesCupang from "../Toples/ToplesCupang";
import ItemInfo from "../ItemInfo/ItemInfo";
import "./boardStyles.css";
import { format, parseISO } from "date-fns";
import { id } from "date-fns/locale";
import ButtonLogout from "../ButtonLogout/ButtonLogout";
import useAxiosJwt from "../../hooks/useAxiosJwt";
import { useNavigate } from "react-router-dom";
import checkLogin from "../../utils/checkLogin";

const data_default = {
  link: {
    title: "Monitoring pH Air",
    type: "link",
    navigateTo: "monitoring-ph",
  },
  items: ["Nilai pH Saat Ini", "Level pH"],
};
const data_pakan_default = {
  link: {
    title: "Monitoring Pakan",
    type: "link",
    navigateTo: "monitoring-pakan",
  },
  items: ["Sisa Pakan Ikan", "Penggunaan Pakan Terakhir"],
};

export default function InfoBoard() {
  const [dataPh, setDataPh] = useState(data_default);
  const [dataPakan, setDataPakan] = useState(data_pakan_default);
  const [pHValue, setPhValue] = useState(0);
  const [levelPh, setLevelPh] = useState("");
  const api = useAxiosJwt(new Date().getTime());
  const [loading, setLoading] = useState(true);
  const bodyInfo = useRef();
  const navigate = useNavigate();

  useEffect(() => {
    (async () => {
      // cek pakah sudah login apa belum
      const isLogin = await checkLogin();
      // jika sudah login ambil data di server
      if (isLogin) {
        setLoading(false);
        fetchPHRecords();
        fetchFeedRecords();
      } else {
        // jika belum login redirect ke halaman login
        navigate("/");
      }
    })();
    // eslint-disable-next-line
  }, []);

  // sebuah fungsi untuk mengambil data pH yang terbaru dari server
  const fetchPHRecords = async () => {
    try {
      const response = await api.get("/ph/CurrentpHValue");
      const data = response.data;
      setPhValue(data.nilaiPh);
      setLevelPh(data.levelPh);
      setDataPh({
        ...data_default,
        values: [[data.nilaiPh], [data.levelPh]],
      });
    } catch (error) {
      console.log("Error maseh");
    }
  };
  // end function

  // sebuah fungsi untuk mengambil data pakan terbaru dari server
  const fetchFeedRecords = async () => {
    try {
      const response = await api.get("/feed/ShortRecords");
      const data = response.data;
      const waktu_pakan = format(
        parseISO(data.info.waktuPakan),
        "eeeeeee, dd MMM yyyy HH:mm:ss",
        { locale: id }
      );
      setDataPakan({
        ...data_pakan_default,
        values: [
          [`${data.info.sisaPakan}%`],
          [`${data.info.beratPakan} gr`, waktu_pakan],
        ],
      });
    } catch (error) {
      console.log("Error maseh");
    }
  };
  // end function

  // scroll function
  let pos = { top: 0, left: 0, x: 0, y: 0 };

  const mouseDownHandler = function (e) {
    if (bodyInfo.current.offsetWidth < 600) {
      bodyInfo.current.style.cursor = "grabbing";
      bodyInfo.current.style.userSelect = "none";
    }

    pos = {
      left: bodyInfo.current.scrollLeft,
      top: bodyInfo.current.scrollTop,
      // Get the current mouse position
      x: e.clientX,
      y: e.clientY,
    };

    document.addEventListener("mousemove", mouseMoveHandler);
    document.addEventListener("mouseup", mouseUpHandler);
  };

  const mouseMoveHandler = function (e) {
    // How far the mouse has been moved
    const dx = e.clientX - pos.x;
    const dy = e.clientY - pos.y;

    // Scroll the element
    bodyInfo.current.scrollTop = pos.top - dy;
    bodyInfo.current.scrollLeft = pos.left - dx;
  };

  const mouseUpHandler = function () {
    if (bodyInfo.current.offsetWidth < 600) {
      bodyInfo.current.style.cursor = "grab";
    } else {
      bodyInfo.current.style.cursor = "default";
    }
    bodyInfo.current.style.removeProperty("user-select");

    document.removeEventListener("mousemove", mouseMoveHandler);
    document.removeEventListener("mouseup", mouseUpHandler);
  };
  // end scroll function

  return loading ? (
    <div></div>
  ) : (
    <div className="info-board">
      <ButtonLogout />
      <div className="item-some-info">
        <h4 className="info-title">Sedikit Informasi</h4>
        <div
          className="info-body"
          ref={bodyInfo}
          onMouseDown={mouseDownHandler}
        >
          <div className="wrap-info">
            <ItemInfo data={dataPh} titleInfo={"Kondisi Ph Air Tambak"} />
            <ItemInfo titleInfo={"Kondisi Pakan Ikan"} data={dataPakan} />
          </div>
        </div>
      </div>
      <div className="item-toples">
        <ToplesCupang
          value={pHValue ? `${pHValue}` : ""}
          level={levelPh ? `${levelPh.toLowerCase()}` : ""}
        />
      </div>
    </div>
  );
}
