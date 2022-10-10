import axios from "axios";
import { BASE_URL } from "./baseUrl";

// function check login
const checkLogin = async () => {
  try {
    const response = await axios.get(`${BASE_URL}/auth/GetAccessToken`, {
      withCredentials: true,
      headers: {
        "Content-Type": "application/json",
      },
    });
    return response.data.accessToken;
  } catch (error) {
    return false;
  }
};
export default checkLogin;
