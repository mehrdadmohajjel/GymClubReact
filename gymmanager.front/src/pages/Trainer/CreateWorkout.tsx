import React from "react";
import { Card, Form, Input, Button, message } from "antd";
import api from "../../api/axios";

const CreateWorkout: React.FC = () => {
    const [form] = Form.useForm();
    const onFinish = async (vals: any) => {
        try {
            await api.post("/workouts/create", vals);
            message.success("برنامه ساخته شد");
            form.resetFields();
        } catch (err: any) {
            message.error("خطا");
        }
    };
    return (
        <Card title="ایجاد برنامه تمرینی">
            <Form form={form} onFinish={onFinish} layout="vertical">
                <Form.Item label="عنوان" name="title" rules={[{ required: true }]}><Input /></Form.Item>
                <Form.Item label="شناسه ورزشکار" name="athleteId" rules={[{ required: true }]}><Input /></Form.Item>
                <Form.Item label="توضیحات (JSON days array)" name="days"><Input.TextArea rows={6} /></Form.Item>
                <Form.Item><Button htmlType="submit" type="primary">ذخیره</Button></Form.Item>
            </Form>
        </Card>
    );
};

export default CreateWorkout;
