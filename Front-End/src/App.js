import React from "react";
import "./App.css";
import InfoBoard from "./component/Board/InfoBoard";
import TitleProject from "./component/Header/TitleProject";
import { Routes, Route } from "react-router-dom";
import MonitoringPh from "./component/MonitoringPh/MonitoringPh";
import MonitroingPakan from "./component/MonitoringPakan/MonitoringPakan";
import Footer from "./component/Footer/Footer";
import Login from "./component/Login/Login";

function App() {
  return (
    <>
      <div className="container">
        <TitleProject />
        <Routes>
          <Route path="/" element={<Login />} />
          <Route path="/dashboard" element={<InfoBoard />} />
          <Route path="/dashboard/monitoring-ph" element={<MonitoringPh />} />
          <Route
            path="/dashboard/monitoring-pakan"
            element={<MonitroingPakan />}
          />
        </Routes>
      </div>
      <Footer />
    </>
  );
}

export default App;
