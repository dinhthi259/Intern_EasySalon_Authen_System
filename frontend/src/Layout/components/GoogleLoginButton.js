import { GoogleLogin } from "@react-oauth/google";
import api from "../../api/AxiosClient";
import { deviceInfo } from "../../helper/GetDeviceInfoHelper";
import { notifyError, notifySuccess } from "../../components/Nofitication";
import { useNavigate } from "react-router-dom";
import { getUserFromToken } from "../../helper/JwtDecodeHelper";

function GoogleLoginButton() {
  const navigate = useNavigate();
  const handleSuccess = async (credentialResponse) => {
    try {
      const res = await api.post("/auth/google", {
        idToken: credentialResponse.credential,
        DeviceInfo: deviceInfo,
      });

      localStorage.setItem("accessToken", res.data.accessToken);
      localStorage.setItem("refreshToken", res.data.refreshToken);
      window.dispatchEvent(new Event("auth-change"));
      const user = getUserFromToken();

      notifySuccess("Đăng nhập thành công");

      // 👉 delay để toast kịp render
      navigate("/");
    } catch (err) {
      console.log("ERROR:", err);

      const errMsg =
        err.response?.data?.message || "Tài khoản của bạn đã bị vô hiệu hóa";

      notifyError(errMsg);
    }
  };

  return (
    <GoogleLogin
      onSuccess={handleSuccess}
      onError={() => notifyError("Login Failed")}
      useOneTap={false}
    />
  );
}

export default GoogleLoginButton;
