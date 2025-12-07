import React from "react";
import { Card, Form, Input, Button, message } from "antd";
import api from "../../api/axios";

const ForgotPassword: React.FC = () => {
    const onFinish = async (values: any) => {
        try {
            await api.post("/auth/forgot", { nationalCode: values.nationalCode });
            message.success("درخواست بازیابی ثبت شد. راهنما ارسال خواهد شد.");
        } catch (err: any) {
            message.error(err?.response?.data?.error ?? "خطا");
        }
    };

    return (
        <div style={{ display: "flex", justifyContent: "center", marginTop: 60 }}>
            <Card style={{ width: 420 }}>
                <h2>بازیابی رمز عبور</h2>
                <Form onFinish={onFinish}>
                    <Form.Item label="کد ملی" name="nationalCode" rules={[{ required: true }]}>
                        <Input />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit">درخواست</Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default ForgotPassword;
