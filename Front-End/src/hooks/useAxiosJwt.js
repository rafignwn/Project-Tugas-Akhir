import axios from "axios";
import jwtDecode from "jwt-decode";
import { useState } from "react";
import { BASE_URL } from "../utils/baseUrl";

export default function useAxiosJwt() {
  const [expire, setExpire] = useState(0);
  const [token, setToken] = useState("");

  const axiosInstance = axios.create({
    baseURL: BASE_URL,
    withCredentials: true,
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
  });

  // membuat axios interceptor untuk mengirimkan access token baru keheader
  // saat access token sudah expire ketika akan melakukan request api
  axiosInstance.interceptors.request.use(
    async (config) => {
      const date = new Date();
      if (expire * 1000 < date.getTime()) {
        const response = await axios.get(`${BASE_URL}/auth/GetAccessToken`, {
          withCredentials: true,
          headers: {
            "Content-Type": "application/json",
          },
        });
        config.headers.Authorization = `Bearer ${response.data.accessToken}`;
        setToken(response.data.accessToken);
        const decoded = await jwtDecode(response.data.accessToken);
        setExpire(decoded.exp);
      }

      return config;
    },
    (error) => {
      return Promise.reject(error);
    }
  );

  return axiosInstance;
}
