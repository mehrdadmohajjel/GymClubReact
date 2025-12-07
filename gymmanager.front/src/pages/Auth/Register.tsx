import React from "react";
import { Card, Form, Input, Button, message } from "antd";
import api from "../../api/axios";

const Register: React.FC = () => {
    const onFinish = async (values: any) => {
        try {
            const resp = await api.post("/auth/register", {
                firstName: values.firstName,
                lastName: values.lastName,
                nationalCode: values.nationalCode,
                phone: values.phone,
                password: values.password
            });
            message.success("ثبت‌نام موفق. اکنون وارد شوید.");
            window.location.href = "/login";
        } catch (err: any) {
            message.error(err?.response?.data?.error ?? "خطا در ثبت‌نام");
        }
    };

    return (
        <div style={{ display: "flex", justifyContent: "center", marginTop: 60 }}>
            <Card style={{ width: 600 }}>
                <h2>ثبت‌نام</h2>
                <Form onFinish={onFinish} layout="vertical" initialValues={{}}>
                    <Form.Item label="نام" name="firstName" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item label="نام خانوادگی" name="lastName" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item label="کد ملی" name="nationalCode" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item label="شماره موبایل" name="phone" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item label="رمز عبور" name="password" rules={[{ required: true, min: 6 }]}>
                        <Input.Password />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">ثبت‌نام</Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default Register;
