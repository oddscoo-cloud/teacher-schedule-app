import React from "react";
import "./HeaderStyles/HeaderStyle.css"

function Header(){
    return (
    <>
        <div className="header">
            <div className="header-logo-section">
                <h1 className="header-title">Менеджер уроків</h1>
            </div>
            
            <div>
                <button className="login-button-header">
                    <a href="/login" className="login-link">Вхід</a>
                </button>
            </div>
        </div>
    </>
    );
}

export default Header;