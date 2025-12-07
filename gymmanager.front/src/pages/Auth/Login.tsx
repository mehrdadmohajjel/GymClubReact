import React, { useContext } from "react";
import { Card, Form, Input, Button, message } from "antd";
import api from "../../api/axios";
import { AuthContext } from "../../contexts/AuthContext";

const Login: React.FC = () => {
    const { login } = useContext(AuthContext);

    const onFinish = async (values: any) => {
        try {
            const resp = await api.post("/auth/login", { nationalCode: values.nationalCode, password: values.password });
            login(resp.data.accessToken, resp.data.refreshToken);
            message.success("ورود موفق");
            window.location.href = "/dashboard";
        } catch (err: any) {
            message.error(err?.response?.data?.error ?? "خطا در ورود");
        }
    };

    return (
        <div style={{ display: "flex", justifyContent: "center", marginTop: 60 }}>
            <Card style={{ width: 420 }}>
                <h2>ورود</h2>
                <Form onFinish={onFinish} layout="vertical">
                    <Form.Item label="کد ملی" name="nationalCode" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item label="رمز عبور" name="password" rules={[{ required: true }]}>
                        <Input.Password />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">ورود</Button>
                        <Button style={{ marginLeft: 8 }} onClick={() => window.location.href = "/register"}>ثبت‌نام</Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default Login;
