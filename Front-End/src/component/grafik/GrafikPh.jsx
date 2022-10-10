import React, { useEffect, useMemo, useState } from "react";
import "./grafikStyles.css";
import { BASE_URL } from "../../utils/baseUrl";
import jwtDecode from "jwt-decode";
import {
  Chart as ChartJS,
  CategoryScale,
  TimeScale,
  LinearScale,
  LineElement,
  Title,
  Tooltip,
  PointElement,
  Legend,
  TimeSeriesScale,
  Filler,
} from "chart.js";
import { Line } from "react-chartjs-2";
import { format, parseISO } from "date-fns";
import { id } from "date-fns/locale";
import axios from "axios";

ChartJS.register(
  CategoryScale,
  Filler,
  TimeSeriesScale,
  TimeScale,
  LinearScale,
  LineElement,
  Title,
  Tooltip,
  PointElement,
  Legend
);

const DATA_END_POINT_API_PH = [
  {
    endPoint: "/ph/RecordPhValueLastDay",
    isActive: false,
    value: "hari",
  },
  {
    endPoint: "/ph/RecordPhValueLastWeek",
    isActive: false,
    value: "minggu",
  },
  {
    endPoint: "/ph/RecordPhValueLastMonth",
    isActive: false,
    value: "bulan",
  },
  // {
  //   endPoint: "/ph/RecordPhValueLastYear",
  //   isActive: false,
  //   value: "tahun",
  // },
  {
    endPoint: "/ph/GetPhValue",
    isActive: true,
    value: "semua",
  },
];

export default function GrafikPh() {
  const [dataPh, setData] = useState([]);
  const [endPoint, setEndPoint] = useState("/ph/GetPhValue");
  const [dataButton, setDataButton] = useState(DATA_END_POINT_API_PH);

  useEffect(() => {
    // fetch data pH
    let token = false;
    const fetchDataPh = async () => {
      try {
        // mengambil data expire token
        const expire = parseInt(localStorage.getItem("expireToken"));
        // cek expire token;
        if (expire * 1000 < new Date().getTime()) {
          // jika sidah expire buat token baru
          token = await fetchToken();
        } else if (!token) {
          // jika token tidak ada buat token baru
          token = await fetchToken();
        }

        const response = await axios.get(BASE_URL + endPoint, {
          withCredentials: true,
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
          },
        });
        setData(response.data);
      } catch (error) {
        console.log(`Terjadi Error : ${error}`);
      }
    };
    fetchDataPh();
    let intervalPh = setInterval(fetchDataPh, 5000);
    return function () {
      clearInterval(intervalPh);
    };
  }, [endPoint]);

  // fetch acctoken
  const fetchToken = async () => {
    const response = await axios.get(`${BASE_URL}/auth/GetAccessToken`, {
      withCredentials: true,
      headers: {
        "Content-Type": "application/json",
      },
    });
    const decoded = jwtDecode(response.data.accessToken);
    localStorage.setItem("expireToken", decoded.exp);
    return response.data.accessToken;
  };

  const dataGrafik = {
    labels: [],
    datasets: [
      {
        label: "Nilai pH Tambak ",
        data: [],
        fill: true,
        borderColor: "#71c7ec",
        backgroundColor: function (context) {
          const chart = context.chart;
          const { ctx, chartArea } = chart;
          if (!chartArea) {
            return;
          }

          const gradient = ctx.createLinearGradient(
            0,
            chartArea.top,
            0,
            chartArea.bottom
          );
          gradient.addColorStop(0, "#107dac99");
          gradient.addColorStop(0.6, "#189ad38f");
          gradient.addColorStop(1, "#f6546a38");

          return gradient;
        },
        borderWidth: 2,
        pointBorderWidth: 0,
        border: false,
        hoverPointBorderWidth: 4,
      },
    ],
  };

  useMemo(() => {
    dataGrafik.datasets[0].data = dataPh.map((dt) => dt.nilaiPh);
    dataGrafik.labels = dataPh.map((dt) => {
      const x = parseISO(dt.waktu_input);
      return format(x, "eeeeeeeeee, d MMM, yyyy ( hh:mm:ss )", {
        locale: id,
      });
    });
    // eslint-disable-next-line
  }, [dataPh]);

  const getLabel = (item, index) => {
    if (index <= 2) {
      return item.length === 4 ? item.slice(0, 3) : item;
    }
  };

  const changeUrlAndAddActive = (paramEndPoint) => {
    setEndPoint(paramEndPoint);
    const dataButtonBaru = DATA_END_POINT_API_PH.map((item) =>
      paramEndPoint === item.endPoint
        ? { endPoint: item.endPoint, isActive: true, value: item.value }
        : { endPoint: item.endPoint, isActive: false, value: item.value }
    );
    setDataButton(dataButtonBaru);
  };

  return (
    <div className="wrapper-chart">
      <div className="btn-chart">
        {dataButton.map((item, index) => (
          <button
            key={index}
            onClick={() => {
              changeUrlAndAddActive(item.endPoint);
            }}
            className={item.isActive ? "active" : ""}
          >
            {item.value}
          </button>
        ))}
      </div>
      <div className="chart-ph">
        <Line
          id="ChartPhRes"
          options={{
            maintainAspectRatio: false,
            elements: {
              line: {
                fill: true,
                backgroundColor: "rgba(25, 25, 25, 0.8)",
                tension: 0.2,
              },
              point: {
                radius: 0,
              },
            },
            interaction: {
              intersect: false,
            },
            responsive: true,
            color: "#d2d2d2",
            scales: {
              y: {
                max: 14,
                min: 0,
                grid: {
                  color: "#2d407d59",
                },
                ticks: {
                  padding: 10,
                  labelOffset: 8,
                  align: "end",
                  color: "#d2d2d2",
                },
              },
              x: {
                grid: {
                  color: "#2d407d59",
                  tickColor: "transparent",
                },
                ticks: {
                  padding: 7,
                  color: "#d2d2d2",
                  autoSkip: true,
                  display: true,
                  align: "start",
                  maxTicksLimit: 3,
                  labelOffset: 2,
                  crossAlign: "near",
                  maxRotation: 0,
                  callback: function (val) {
                    // Hide every 2nd tick label
                    let label = this.getLabelForValue(val).split(" ");
                    label = label.map(getLabel);
                    return label.join(" ");
                  },
                },
              },
            },
            plugins: {
              legend: {
                position: "top",

                labels: {
                  boxWidth: 15,
                  boxHeight: 15,
                  boxPadding: 10,
                },
              },
              title: {
                display: true,
                text: "Grafik Nilai pH Air Tambak Ikan Bandeng",
                color: "#d2d2d2",
              },
              tooltip: {
                padding: 10,
                bodyFont: {
                  weight: 500,
                  size: 13,
                  lineHeight: 2,
                },
              },
            },
          }}
          data={dataGrafik}
        />
      </div>
    </div>
  );
}
