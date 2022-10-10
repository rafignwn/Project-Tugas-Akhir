import React, { useEffect, useState } from "react";
import GrafikPh from "../grafik/GrafikPh";
import LinkHome from "../LinkHome/LinkHome";
import InfoMonitoring from "./InfoMonitoring";
import "./monitoringPhStyles.css";
import checkLogin from "../../utils/checkLogin";
import { useNavigate } from "react-router-dom";
import ButtonLogout from "../ButtonLogout/ButtonLogout";

export default function MonitoringPh() {
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  useEffect(() => {
    (async () => {
      // cek pakah sudah login apa belum
      const isLogin = await checkLogin();
      // jika sudah login ambil data di server
      if (isLogin) {
        setLoading(false);
      } else {
        // jika belum login redirect ke halaman login
        navigate("/");
      }
    })();
  }, [navigate]);

  return loading ? (
    <div></div>
  ) : (
    <div className="mt-30">
      <ButtonLogout />
      <LinkHome />
      <div className="monitoring-ph">
        <GrafikPh />
        <InfoMonitoring />
      </div>
    </div>
  );
}
