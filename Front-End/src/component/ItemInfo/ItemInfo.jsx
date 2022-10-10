import React from "react";
import { Link } from "react-router-dom";
import "./ItemInfoStyles.css";

export default function ItemInfo({ data, titleInfo }) {
  return (
    <div className="info">
      <h5>{titleInfo}</h5>
      <ul>
        {data.items?.map((item, index) => {
          return (
            typeof item === "string" && (
              <li key={index}>
                {item} <br />
                {data.values ? (
                  data.values[index]?.map((item, index) => (
                    <span
                      className={
                        typeof item === "string"
                          ? item.toLowerCase().split(" ")[0]
                          : ""
                      }
                      key={index}
                    >
                      {typeof item === "string" ? item.toLowerCase() : item}
                    </span>
                  ))
                ) : (
                  <span>Loading...</span>
                )}
              </li>
            )
          );
        })}
      </ul>
      {data.link && (
        <div className="link">
          {data.link.type === "button" ? (
            <button onClick={data.link.action}>{data.link.title}</button>
          ) : (
            <Link to={data.link.navigateTo}>{data.link.title}</Link>
          )}
        </div>
      )}
    </div>
  );
}
