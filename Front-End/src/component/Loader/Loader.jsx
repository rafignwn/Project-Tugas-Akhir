import React from "react";
import "./Loading.css";

export default function Loader() {
  return (
    <div className="loading">
      <div className="circle">
        <span
          style={{
            "--i": 1,
          }}
        ></span>
        <span
          style={{
            "--i": 2,
          }}
        ></span>
        <span
          style={{
            "--i": 3,
          }}
        ></span>
        <span
          style={{
            "--i": 4,
          }}
        ></span>
      </div>
    </div>
  );
}
