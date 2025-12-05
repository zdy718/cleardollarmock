import { useEffect, useState } from 'react';
import { Navbar } from "../Components/Navbar";

export function TransactionsPage() {
    const [transactions, setTransactions] = useState();

    useEffect(() => {
        populateTransactions();
    }, []);

    const contents = transactions === undefined
        ? <p className="text-center"><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <div className="d-flex justify-content-center">
            <table className="table mx-auto border-separate border-spacing-x-6" aria-labelledby="tableLabel">
                <thead>
                    <tr>
                        <th className="px-2 text-left">TransactionId</th>
                        <th className="px-2 text-left">TagId</th>
                        <th className="px-2 text-left">Date</th>
                        <th className="px-2 text-left">Amount</th>
                        <th className="px-2 text-left">MerchantDetails</th>

                    </tr>
                </thead>
                <tbody>
                    {transactions.map((transaction: any) =>
                        <tr key={transaction.transactionId}>
                            <td className="px-2">{transaction.transactionId}</td>
                            <td className="px-2">{transaction.tagId}</td>
                            <td className="px-2">{transaction.date}</td>
                            <td className="px-2">{transaction.amount}</td>
                            <td className="px-2">{transaction.merchantDetails}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>;

    return (
        <>
            <Navbar />
            <div className="flex items-center justify-center gap-4 my-2">
                <button className="px-3 py-1.5 border rounded-lg hover:bg-gray-50">Import CSV</button>
                <button className="px-3 py-1.5 border rounded-lg hover:bg-gray-50">Connect Bank</button>
            </div>
            <div className="container">
                <h1 id="tableLabel" className="text-center">All Transactions</h1>
                <p className="text-center">This component demonstrates fetching data from the server.</p>
                {contents}
            </div>
        </>
    );

    async function populateTransactions() {
        const response = await fetch('/transactions?userId=demo-user');
        if (response.ok) {
            const data = await response.json();
            setTransactions(data);
        }
    }

}