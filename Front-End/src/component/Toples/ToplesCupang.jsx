import React from "react";
import "./toplesStyles.css";

export default function ToplesCupang({ value, level }) {
  return (
    <div className="section">
      <div className="shadow"></div>
      <div className="toples" id="wrapFix">
        <div className={`${level} liquid`}>
          {value && (
            <span>
              <code>pH</code>
              {value}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
