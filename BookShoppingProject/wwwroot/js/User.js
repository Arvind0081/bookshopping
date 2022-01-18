var dataTable;

$(document).ready(function(){
    loadDataTable();
})

function loadDataTable() {

    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url": "/Admin/User/GetAll"
        },

        "columns": [
            { "data": "name", "width": "20%", "className": "dt-center" },
            { "data": "email", "width": "20%", "className": "dt-center" },
            { "data": "phoneNumber", "width": "20%", "className": "dt-center" },
            { "data": "company.name", "width": "20%", "className": "dt-center" },
            { "data": "role", "width": "20%", "className": "dt-center" },
            {
                "data":
                {
                    id: "id", lockoutEnd: "lockoutEnd"
                },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        //user locked
                        return `
                               <div class="text-center">
                               <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white">Unlock</a>
                              </div>
                                   `;
                    }
                    else
                    {
                        // user Unlock
                        return `
                               <div class="text-center">
                               <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white">Lock</a>
                              </div>
                                   `;
                    }
                }

            }
        ]
    })
}

function LockUnlock(id) {

    $.ajax({
        type: "POST",
        url: "/Admin/User/LockUnlock",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    })
}