$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    $('#tblData').DataTable({
        ajax: {
            url: '/admin/order/getall',
            dataSrc: 'data' // This matches your { data: objProductList } structure
        },
        columns: [
            { data: 'id', width: '5%' },
            { data: 'name', width: '20%' },
            { data: 'phoneNumber', width: '20%' },
            { data: 'applicationUser.email', width: '10%' },
            { data: 'orderStatus', width: '10%' },
            { data: 'orderTotal', width: '10%' },
            {
                data: 'category.categoryName',
                width: '15%',
                defaultContent: 'N/A' // Shows when category is null
            },
            {
                data: 'productId',
                render: function (data) {
                    return `
            <div class="btn-group">
                <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2">
                    <i class="bi bi-pencil-square"></i>
                </a>
           
            </div>`;
                }
            }
        ],
        initComplete: function () {
            console.log('DataTable initialized'); // Debugging
        },
        error: function (xhr, error, thrown) {
            console.error('Error loading data:', error, thrown);
        }
    });
}