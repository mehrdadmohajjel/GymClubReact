import React from "react";
import { Card } from "antd";
import { useSearchParams } from "react-router-dom";

const Result: React.FC = () => {
    const [params] = useSearchParams();
    const success = params.get("success");
    return (
        <Card title="نتیجه پرداخت">
            {success === "true" ? <div>پرداخت با موفقیت انجام شد.</div> : <div>پرداخت ناموفق بود.</div>}
        </Card>
    );
};

export default Result;
