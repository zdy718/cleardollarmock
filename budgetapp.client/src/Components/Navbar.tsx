
import { Link } from "react-router-dom";
export function Navbar() {
    return (
        <>
            {/* top bar */}
            <div className="sticky top-0 bg-white border-b">
                <div className="max-w-6xl mx-auto px-4 py-3 flex items-center">
                    <div className="font-bold text-xl">ClearDollar</div>
                    <div className="ml-auto flex gap-2">
                        <Link to="/dashboard">
                            <button className="px-3 py-1.5 border rounded-lg hover:bg-gray-50">Dashboard</button>
                        </Link>
                        <Link to="/budget">
                            <button className="px-3 py-1.5 border rounded-lg hover:bg-gray-50">Budget</button>
                        </Link>
                        <Link to="/transactions">
                            <button className="px-3 py-1.5 border rounded-lg hover:bg-gray-50">Transactions</button>
                        </Link>
                    </div>
                </div>
            </div>
        </>
    )
}