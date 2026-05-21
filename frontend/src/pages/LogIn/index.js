import { useState } from "react";
import styles from "./LogIn.module.scss";
import classNames from "classnames/bind";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faEnvelope,
  faEye,
  faEyeSlash,
} from "@fortawesome/free-regular-svg-icons";
import { faLock } from "@fortawesome/free-solid-svg-icons";
import { Link, useNavigate } from "react-router-dom";
import { logIn } from "../../api/AuthApi";
import { notifySuccess, notifyError } from "../../components/Nofitication";
import GoogleLoginButton from "../../Layout/components/GoogleLoginButton";
import { loginPasskey } from "../../api/PasskeyApi";
import { deviceInfo } from "../../helper/GetDeviceInfoHelper";
import { getUserFromToken } from "../../helper/JwtDecodeHelper";

const cx = classNames.bind(styles);

function LogIn() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const [rememberChecked, setRememberChecked] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleLogin = async () => {
    setLoading(true);
    try {
      const res = await logIn(email, password, deviceInfo);
      if (res) {
        notifySuccess("Đăng nhập thành công");
        localStorage.setItem("accessToken", res.data.accessToken);
        localStorage.setItem("refreshToken", res.data.refreshToken);
        window.dispatchEvent(new Event("auth-change"));
        const user = getUserFromToken();
        navigate("/");
      }
    } catch (error) {
      const errorMsg = error.response?.data || "Đăng nhập thất bại";
      notifyError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const handlePasskeyLogin = async () => {
    setLoading(true);
    try {
      const res = await loginPasskey();
      if (res) {
        notifySuccess("Đăng nhập bằng Passkey thành công");
        const user = getUserFromToken();
        navigate("/"); // redirect luôn
      }
    } catch (error) {
      notifyError(error.response?.data || "Đăng nhập thất bại");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={cx("wrapper")}>
      {/* LEFT IMAGE */}
      <div className={cx("left")}>
        <img
          src={require("../../assets/Login_img/login_img.png")}
          alt="login"
        />
      </div>

      {/* RIGHT FORM */}
      <div className={cx("right")}>
        <div className={cx("container")}>
          <div className={cx("header_title")}>
            <div className={cx("welcome_title")}>
              <h1>Chào mừng trở lại!</h1>
              <p>Đăng nhập bằng tài khoản bạn để tiếp tục</p>
            </div>
          </div>

          <div className={cx("input_container")}>
            {/* EMAIL */}
            <div className={cx("input_email_container")}>
              <h4>Email</h4>
              <div className={cx("input_email")}>
                <div className={cx("input_icon")}>
                  <FontAwesomeIcon icon={faEnvelope} />
                </div>
                <input
                  type="text"
                  autoComplete="username"
                  style={{ display: "none" }}
                />
                <input
                  type="email"
                  placeholder="you@gmail.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>
            </div>

            {/* PASSWORD */}
            <div className={cx("input_password_container")}>
              <div className={cx("password-title")}>
                <h4>Mật khẩu</h4>
                <Link to="/input-email-reset">Quên mật khẩu</Link>
              </div>

              <div className={cx("input_password")}>
                <div className={cx("input_icon")}>
                  <FontAwesomeIcon icon={faLock} />
                </div>
                <input type="password" style={{ display: "none" }} />
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="● ● ● ● ● ●"
                  value={password}
                  autoComplete="new-password"
                  onChange={(e) => setPassword(e.target.value)}
                />
                <i onClick={() => setShowPassword(!showPassword)}>
                  <FontAwesomeIcon
                    className={cx("eye")}
                    icon={showPassword ? faEyeSlash : faEye}
                  />
                </i>
              </div>
            </div>
          </div>

          {/* REMEMBER */}
          <div className={cx("remember_me")}>
            <input
              className={cx("checkbox")}
              type="checkbox"
              checked={rememberChecked}
              onChange={(e) => setRememberChecked(e.target.checked)}
            />
            Lưu mật khẩu cho lần đăng nhập tới
          </div>

          {/* LOGIN BTN */}
          <div className={cx("signin-button")}>
            <button
              className={cx("button", "signin-btn")}
              onClick={handleLogin}
            >
              {loading ? (
                <>
                  <span className={cx("spinner")} />
                  Đang xử lý...
                </>
              ) : (
                "Đăng nhập"
              )}
            </button>
          </div>

          {/* SOCIAL */}
          <div className={cx("divider")}>
            <span>Hoặc đăng nhập với</span>
          </div>

          <div className={cx("signin-button")}>
            <div className={cx("button", "google-btn")}>
              <GoogleLoginButton />
            </div>

            <button
              className={cx("button", "passkey-btn")}
              onClick={handlePasskeyLogin}
            >
              <img
                src="https://img.icons8.com/?size=100&id=21602&format=png&color=000000"
                alt="google"
              />
              <span>Đăng nhập bằng Passkey</span>
            </button>
          </div>

          {/* REGISTER */}
          <div className={cx("signup-link")}>
            <span>Bạn chưa có tài khoản? </span>
            <Link to="/register">Đăng ký</Link>
          </div>
        </div>
      </div>
    </div>
  );
}

export default LogIn;
