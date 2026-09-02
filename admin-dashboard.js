
/* ==========================================================
   APPLICANT COURSE FILTER
   ========================================================== */

function filterDashboardApplicants() {
    var course = document.getElementById('dashboard-course-filter').value;
    var rows = document.querySelectorAll('.dashboard-applicant-row');
    var visible = 0;

    rows.forEach(function (row) {
        var match =
            course === 'all' ||
            row.getAttribute('data-course') === course;

        row.classList.toggle('hidden', !match);

        if (match) {
            visible++;
        }
    });

    document
        .getElementById('dashboard-no-results')
        .classList.toggle('hidden', visible > 0);

    document.getElementById('dashboard-showing-count').textContent =
        'Showing ' + visible + ' of ' + rows.length + ' applicants';
}


    /* ==========================================================
       BATCH MANAGEMENT
       ========================================================== */

    (function () {


        var STORAGE_KEY =
            'abec-admin-batches-v2';

        var LEGACY_STORAGE_KEY =
            'abec-admin-batches';


        var pendingSelectId = null;

        var createBatchContext = null;



        /* ======================================================
           DEFAULT STATE
           ====================================================== */

        function defaultState() {

            return {
                activeId: null,
                batches: []
            };

        }



        /* ======================================================
           LOAD STATE
           ====================================================== */

        function loadState() {

            try {

                try {

                    localStorage.removeItem(
                        LEGACY_STORAGE_KEY
                    );

                } catch (e) {

                    /* Ignore */

                }


                var raw =
                    localStorage.getItem(
                        STORAGE_KEY
                    );


                if (!raw) {
                    return defaultState();
                }


                var parsed =
                    JSON.parse(raw);


                if (
                    !parsed ||
                    !Array.isArray(parsed.batches)
                ) {

                    return defaultState();

                }


                return parsed;


            } catch (e) {

                return defaultState();

            }

        }



        /* ======================================================
           SAVE STATE
           ====================================================== */

        function saveState(state) {

            try {

                localStorage.setItem(
                    STORAGE_KEY,
                    JSON.stringify(state)
                );

            } catch (e) {

                console.error(
                    'Unable to save batch state:',
                    e
                );

            }

        }



        /* ======================================================
           GET STATE
           ====================================================== */

        function getState() {

            return loadState();

        }



        /* ======================================================
           FORMAT DATE
           ====================================================== */

        function formatDate(iso) {

            if (!iso) {
                return '—';
            }


            var d =
                new Date(
                    iso + 'T00:00:00'
                );


            if (isNaN(d.getTime())) {
                return iso;
            }


            return d.toLocaleDateString(
                'en-US',
                {
                    month: 'short',
                    day: '2-digit',
                    year: 'numeric'
                }
            );

        }



        /* ======================================================
           GET ACTIVE BATCH
           ====================================================== */

        function getActiveBatch(state) {

            return state.batches.find(
                function (batch) {

                    return batch.id === state.activeId;

                }
            ) || null;

        }



        /* ======================================================
           ESCAPE HTML
           ====================================================== */

        function escapeHtml(str) {

            return String(str)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#039;');

        }



        /* ======================================================
           RENDER ACTIVE BATCH
           ====================================================== */

        function renderActiveBatch() {

            var state =
                getState();

            var active =
                getActiveBatch(state);


            var completeBtn =
                document.getElementById(
                    'btn-complete-batch'
                );


            var nameEl =
                document.getElementById(
                    'active-batch-name'
                );

            var yearEl =
                document.getElementById(
                    'active-batch-year'
                );

            var studentsEl =
                document.getElementById(
                    'active-batch-students'
                );

            var startEl =
                document.getElementById(
                    'active-batch-start'
                );

            var endEl =
                document.getElementById(
                    'active-batch-end'
                );

            var statusEl =
                document.getElementById(
                    'active-batch-status'
                );

            var completeLabel =
                document.getElementById(
                    'complete-batch-label'
                );


            /* ----------------------------------------------
               NO ACTIVE BATCH
               ---------------------------------------------- */

            if (!active) {

                if (nameEl) {
                    nameEl.textContent =
                        'No Active Batch';
                }

                if (yearEl) {
                    yearEl.textContent = '—';
                }

                if (studentsEl) {
                    studentsEl.textContent = '0';
                }

                if (startEl) {
                    startEl.textContent = '—';
                }

                if (endEl) {
                    endEl.textContent = '—';
                }

                if (statusEl) {

                    statusEl.innerHTML =
                        '<span class="w-1.5 h-1.5 rounded-full bg-slate-400"></span> None';

                    statusEl.className =
                        'inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold bg-slate-100 text-slate-600';

                }

                if (completeBtn) {
                    completeBtn.disabled = true;
                }

                renderHistory();

                return;
            }



            /* ----------------------------------------------
               ACTIVE BATCH
               ---------------------------------------------- */

            if (nameEl) {
                nameEl.textContent =
                    active.name;
            }

            if (yearEl) {
                yearEl.textContent =
                    active.year;
            }

            if (studentsEl) {
                studentsEl.textContent =
                    String(active.students);
            }

            if (startEl) {
                startEl.textContent =
                    formatDate(active.start);
            }

            if (endEl) {
                endEl.textContent =
                    formatDate(active.end);
            }

            if (completeLabel) {
                completeLabel.textContent =
                    active.name;
            }


            if (statusEl) {

                statusEl.innerHTML =
                    '<span class="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse"></span> Active';

                statusEl.className =
                    'inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold bg-green-100 text-green-700';

            }


            if (completeBtn) {
                completeBtn.disabled = false;
            }


            renderHistory();

        }



        /* ======================================================
           RENDER BATCH HISTORY
           ====================================================== */

        function renderHistory() {

            var state =
                getState();


            var archived =
                state.batches
                    .filter(function (batch) {

                        return batch.status === 'Completed';

                    })
                    .sort(function (a, b) {

                        return String(
                            b.completedAt || b.end
                        ).localeCompare(
                            String(
                                a.completedAt || a.end
                            )
                        );

                    });


            var body =
                document.getElementById(
                    'batch-history-body'
                );

            var empty =
                document.getElementById(
                    'batch-history-empty'
                );

            var count =
                document.getElementById(
                    'batch-history-count'
                );


            if (!body || !empty || !count) {
                return;
            }


            count.textContent =
                archived.length +
                ' archived';


            if (!archived.length) {

                body.innerHTML = '';

                empty.classList.remove(
                    'hidden'
                );

                return;
            }


            empty.classList.add(
                'hidden'
            );


            body.innerHTML =
                archived.map(
                    function (batch) {

                        return (

                            '<tr class="hover:bg-slate-50/50 transition-colors">' +

                                '<td class="px-6 py-3">' +

                                    '<p class="text-sm font-semibold text-slate-800">' +
                                        escapeHtml(batch.name) +
                                    '</p>' +

                                '</td>' +

                                '<td class="px-6 py-3 text-sm text-slate-600">' +
                                    escapeHtml(batch.year) +
                                '</td>' +

                                '<td class="px-6 py-3 text-sm text-slate-600">' +
                                    escapeHtml(batch.students) +
                                '</td>' +

                                '<td class="px-6 py-3 text-sm text-slate-600">' +
                                    formatDate(batch.start) +
                                    ' – ' +
                                    formatDate(batch.end) +
                                '</td>' +

                                '<td class="px-6 py-3 text-sm text-slate-600">' +
                                    formatDate(
                                        batch.completedAt ||
                                        batch.end
                                    ) +
                                '</td>' +

                                '<td class="px-6 py-3">' +

                                    '<span class="inline-block px-2 py-0.5 rounded text-[10px] font-bold bg-slate-100 text-slate-600">' +
                                        'COMPLETED' +
                                    '</span>' +

                                '</td>' +

                            '</tr>'

                        );

                    }
                ).join('');

        }



        /* ======================================================
           BATCH OPTION HTML
           ====================================================== */

        function batchOptionHtml(
            batch,
            isActive,
            isSelected,
            isArchived
        ) {

            var classes =
                'batch-option w-full text-left rounded-xl border border-slate-200 p-3 transition-all ';


            if (isArchived) {

                classes +=
                    'opacity-70 cursor-default bg-slate-50 ';

            } else {

                classes +=
                    'hover:border-abec-blue/40 hover:bg-slate-50 cursor-pointer ';

            }


            if (isActive) {

                classes +=
                    'active-batch ';

            }


            if (
                isSelected &&
                !isArchived
            ) {

                classes +=
                    'selected ';

            }


            var badge;


            if (isActive) {

                badge =
                    '<span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-green-100 text-green-700">CURRENT</span>';

            } else if (isArchived) {

                badge =
                    '<span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-slate-200 text-slate-600">ARCHIVED</span>';

            } else {

                badge =
                    '<span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-blue-50 text-abec-blue">' +
                    escapeHtml(batch.status) +
                    '</span>';

            }


            var attrs =
                isArchived
                    ? ''
                    : ' onclick="highlightBatch(\'' +
                      escapeHtml(batch.id) +
                      '\')"';


            return (

                '<button type="button" class="' +
                    classes +
                    '"' +
                    attrs +
                    ' data-batch-id="' +
                    escapeHtml(batch.id) +
                    '">' +

                    '<div class="flex items-start justify-between gap-3">' +

                        '<div>' +

                            '<p class="text-sm font-bold text-slate-800">' +
                                escapeHtml(batch.name) +
                            '</p>' +

                            '<p class="text-xs text-slate-500 mt-0.5">' +
                                escapeHtml(batch.year) +
                                ' · ' +
                                escapeHtml(batch.students) +
                                ' students' +
                            '</p>' +

                            '<p class="text-[11px] text-slate-400 mt-1">' +
                                formatDate(batch.start) +
                                ' – ' +
                                formatDate(batch.end) +
                            '</p>' +

                        '</div>' +

                        badge +

                    '</div>' +

                '</button>'

            );

        }



        /* ======================================================
           RENDER CHANGE BATCH LIST
           ====================================================== */

        window.renderChangeBatchList =
            function () {

                var state =
                    getState();


                var searchInput =
                    document.getElementById(
                        'batch-search'
                    );


                if (!searchInput) {
                    return;
                }


                var q =
                    (
                        searchInput.value ||
                        ''
                    )
                    .trim()
                    .toLowerCase();


                var availableEl =
                    document.getElementById(
                        'change-batch-available'
                    );

                var archivedEl =
                    document.getElementById(
                        'change-batch-archived'
                    );


                if (
                    !availableEl ||
                    !archivedEl
                ) {
                    return;
                }


                var available =
                    state.batches.filter(
                        function (batch) {

                            return (
                                batch.status !==
                                'Completed'
                            ) && (
                                !q ||
                                String(batch.name)
                                    .toLowerCase()
                                    .indexOf(q) !== -1 ||

                                String(batch.year)
                                    .toLowerCase()
                                    .indexOf(q) !== -1
                            );

                        }
                    );


                var archived =
                    state.batches.filter(
                        function (batch) {

                            return (
                                batch.status ===
                                'Completed'
                            ) && (
                                !q ||
                                String(batch.name)
                                    .toLowerCase()
                                    .indexOf(q) !== -1 ||

                                String(batch.year)
                                    .toLowerCase()
                                    .indexOf(q) !== -1
                            );

                        }
                    );


                /* Available */

                availableEl.innerHTML =
                    available.length

                        ? available.map(
                            function (batch) {

                                return batchOptionHtml(
                                    batch,
                                    batch.id ===
                                        state.activeId,
                                    batch.id ===
                                        pendingSelectId,
                                    false
                                );

                            }
                        ).join('')

                        : '<p class="text-xs text-slate-400 py-3 text-center">' +
                          'No available batches found.' +
                          '</p>';


                /* Archived */

                archivedEl.innerHTML =
                    archived.length

                        ? archived.map(
                            function (batch) {

                                return batchOptionHtml(
                                    batch,
                                    false,
                                    false,
                                    true
                                );

                            }
                        ).join('')

                        : '<p class="text-xs text-slate-400 py-3 text-center">' +
                          'No archived batches match your search.' +
                          '</p>';


                /* Select button */

                var selectBtn =
                    document.getElementById(
                        'btn-select-batch'
                    );


                if (selectBtn) {

                    var canSelect =
                        pendingSelectId &&
                        pendingSelectId !==
                            state.activeId &&
                        available.some(
                            function (batch) {

                                return (
                                    batch.id ===
                                    pendingSelectId
                                );

                            }
                        );


                    selectBtn.disabled =
                        !canSelect;

                }

            };



        /* ======================================================
           HIGHLIGHT BATCH
           ====================================================== */

        window.highlightBatch =
            function (id) {

                pendingSelectId =
                    id;

                renderChangeBatchList();

            };



        /* ======================================================
           SHOW MODAL
           ====================================================== */

        function showModal(id) {

            var el =
                document.getElementById(id);


            if (!el) {
                return;
            }


            el.classList.remove(
                'hidden'
            );


            el.setAttribute(
                'aria-hidden',
                'false'
            );


            document.body.classList.add(
                'overflow-hidden'
            );

        }



        /* ======================================================
           HIDE MODAL
           ====================================================== */

        function hideModal(id) {

            var el =
                document.getElementById(id);


            if (!el) {
                return;
            }


            el.classList.add(
                'hidden'
            );


            el.setAttribute(
                'aria-hidden',
                'true'
            );


            if (
                !document.querySelector(
                    '[id^="modal-"]:not(.hidden)'
                )
            ) {

                document.body.classList.remove(
                    'overflow-hidden'
                );

            }

        }



        /* ======================================================
           OPEN CHANGE BATCH MODAL
           ====================================================== */

        window.openChangeBatchModal =
            function () {

                var state =
                    getState();


                pendingSelectId =
                    state.activeId;


                var search =
                    document.getElementById(
                        'batch-search'
                    );


                if (search) {
                    search.value = '';
                }


                renderChangeBatchList();


                showModal(
                    'modal-change-batch'
                );

            };



        /* ======================================================
           CLOSE CHANGE BATCH MODAL
           ====================================================== */

        window.closeChangeBatchModal =
            function () {

                hideModal(
                    'modal-change-batch'
                );


                pendingSelectId =
                    null;

            };



        /* ======================================================
           SELECT HIGHLIGHTED BATCH
           ====================================================== */

        window.selectHighlightedBatch =
            function () {

                var state =
                    getState();


                if (
                    !pendingSelectId ||
                    pendingSelectId ===
                        state.activeId
                ) {
                    return;
                }


                var target =
                    state.batches.find(
                        function (batch) {

                            return (
                                batch.id ===
                                pendingSelectId
                            );

                        }
                    );


                if (
                    !target ||
                    target.status ===
                        'Completed'
                ) {
                    return;
                }


                state.batches.forEach(
                    function (batch) {

                        if (
                            batch.id ===
                                state.activeId &&
                            batch.status ===
                                'Active'
                        ) {

                            batch.status =
                                'Ready';

                        }

                    }
                );


                target.status =
                    'Active';


                state.activeId =
                    target.id;


                saveState(state);


                window.closeChangeBatchModal();


                renderActiveBatch();

            };



        /* ======================================================
           OPEN COMPLETE BATCH MODAL
           ====================================================== */

        window.openCompleteBatchModal =
            function () {

                var state =
                    getState();


                var active =
                    getActiveBatch(state);


                if (!active) {
                    return;
                }


                var label =
                    document.getElementById(
                        'complete-batch-label'
                    );


                if (label) {

                    label.textContent =
                        active.name;

                }


                showModal(
                    'modal-complete-batch'
                );

            };



        /* ======================================================
           CLOSE COMPLETE BATCH MODAL
           ====================================================== */

        window.closeCompleteBatchModal =
            function () {

                hideModal(
                    'modal-complete-batch'
                );

            };



        /* ======================================================
           CONFIRM COMPLETE BATCH
           ====================================================== */

        window.confirmCompleteBatch =
            function () {

                var state =
                    getState();


                var active =
                    getActiveBatch(state);


                if (!active) {
                    return;
                }


                active.status =
                    'Completed';


                active.completedAt =
                    new Date()
                        .toISOString()
                        .slice(0, 10);


                state.activeId =
                    null;


                saveState(state);


                window.closeCompleteBatchModal();


                renderActiveBatch();


                window.openCreateBatchModal(
                    'after-complete'
                );

            };

        /* ======================================================
           OPEN CREATE BATCH MODAL
           ====================================================== */

        window.openCreateBatchModal =
            function (context) {

                createBatchContext =
                    context || 'manual';


                var banner =
                    document.getElementById(
                        'create-batch-success-banner'
                    );


                var title =
                    document.getElementById(
                        'create-batch-title'
                    );


                if (
                    createBatchContext ===
                    'after-complete'
                ) {

                    if (banner) {

                        banner.classList.remove(
                            'hidden'
                        );

                    }

                    if (title) {

                        title.textContent =
                            'Create New Batch';

                    }

                } else {

                    if (banner) {

                        banner.classList.add(
                            'hidden'
                        );

                    }

                    if (title) {

                        title.textContent =
                            'Create New Batch';

                    }

                }


                var form =
                    document.getElementById(
                        'create-batch-form'
                    );


                if (form) {
                    form.reset();
                }


                if (
                    createBatchContext ===
                    'from-change'
                ) {

                    hideModal(
                        'modal-change-batch'
                    );

                }


                showModal(
                    'modal-create-batch'
                );

            };



        /* ======================================================
           CLOSE CREATE BATCH MODAL
           ====================================================== */

        window.closeCreateBatchModal =
            function () {

                var wasFromChange =
                    createBatchContext ===
                    'from-change';


                hideModal(
                    'modal-create-batch'
                );


                createBatchContext =
                    null;


                if (wasFromChange) {

                    showModal(
                        'modal-change-batch'
                    );


                    renderChangeBatchList();

                }

            };



        /* ======================================================
           SUBMIT CREATE BATCH
           ====================================================== */

        window.submitCreateBatch =
            function (event) {

                event.preventDefault();


                var nameEl =
                    document.getElementById(
                        'new-batch-name'
                    );

                var yearEl =
                    document.getElementById(
                        'new-batch-year'
                    );

                var startEl =
                    document.getElementById(
                        'new-batch-start'
                    );

                var endEl =
                    document.getElementById(
                        'new-batch-end'
                    );

                var maxEl =
                    document.getElementById(
                        'new-batch-max'
                    );


                if (
                    !nameEl ||
                    !yearEl ||
                    !startEl ||
                    !endEl ||
                    !maxEl
                ) {
                    return;
                }


                var name =
                    nameEl.value.trim();

                var year =
                    yearEl.value.trim();

                var start =
                    startEl.value;

                var end =
                    endEl.value;

                var maxStudents =
                    parseInt(
                        maxEl.value,
                        10
                    );


                if (
                    !name ||
                    !year ||
                    !start ||
                    !end ||
                    !maxStudents ||
                    maxStudents < 1
                ) {

                    return;

                }


                if (
                    new Date(end) <
                    new Date(start)
                ) {

                    alert(
                        'End date must be on or after the start date.'
                    );

                    return;

                }


                var state =
                    getState();


                var id =
                    'b' +
                    Date.now();


                /* Make previous active batch ready */

                state.batches.forEach(
                    function (batch) {

                        if (
                            batch.status ===
                            'Active'
                        ) {

                            batch.status =
                                'Ready';

                        }

                    }
                );


                /* Add new batch */

                state.batches.push({

                    id: id,

                    name: name,

                    year: year,

                    status: 'Active',

                    students: 0,

                    maxStudents:
                        maxStudents,

                    start: start,

                    end: end,

                    completedAt: null

                });


                state.activeId =
                    id;


                saveState(state);


                window.closeCreateBatchModal();


                renderActiveBatch();

            };

        /* ======================================================
           ESC KEY - CLOSE MODALS
           ====================================================== */

        document.addEventListener(
            'keydown',
            function (event) {

                if (
                    event.key !==
                    'Escape'
                ) {
                    return;
                }


                [
                    'modal-change-batch',
                    'modal-complete-batch',
                    'modal-create-batch'
                ].forEach(
                    function (id) {

                        var el =
                            document.getElementById(
                                id
                            );


                        if (
                            el &&
                            !el.classList.contains(
                                'hidden'
                            )
                        ) {

                            hideModal(id);

                        }

                    }
                );

            }
        );



        /* ======================================================
           INITIAL RENDER
           ====================================================== */

        renderActiveBatch();

    })();