import React from "react";
import "./progressStyles.css";

export default function ProgressBar({ value, title }) {
  return (
    <div className="card">
      <div className="box">
        <div>
          <div className="percent">
            <svg>
              <circle cx="70" cy="70" r="70"></circle>
              <circle
                cx="70"
                cy="70"
                r="70"
                style={{
                  strokeDashoffset: `calc(440px - (${value}px * 440) / 100)`,
                }}
              ></circle>
            </svg>
            <div className="number">
              <h2>
                {value}
                <span>%</span>
              </h2>
            </div>
          </div>
        </div>
      </div>
      <div className="text">{title}</div>
    </div>
  );
}
