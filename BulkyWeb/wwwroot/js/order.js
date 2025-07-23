$(document).ready(function () {
    const url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    } else if (url.includes("pending")) {
        loadDataTable("pending");
    } else if (url.includes("completed")) {
        loadDataTable("completed");
    } else if (url.includes("approved")) {
        loadDataTable("approved");
    } else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    $('#tblData').DataTable({
        ajax: {
            url: `/admin/order/getallorders?status=${status}`,
            dataSrc: 'data',
            error: function (xhr) {
                console.error("Error loading data:", xhr.responseText);
                alert("Could not load order data.");
            }
        },
        columns: [
            { data: 'id', width: '5%' },
            { data: 'name', width: '20%' },
            { data: 'phoneNumber', width: '15%' },
            { data: 'email', width: '20%' },  
            { data: 'orderStatus', width: '15%' },
            { data: 'orderTotal', width: '10%' },
            {
                data: 'id',
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/Order/Details/${data}" class="btn btn-primary mx-2">
                                <i class="bi bi-eye-fill"></i> Details
                            </a>
                        </div>`;
                },
                width: '15%'
            }
        ],
        destroy: true
    });
}
