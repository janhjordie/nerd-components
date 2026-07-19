# Coverage matrix — MudBlazor 9.7.0

Legend: **P0** palette-only · **P1** stateful (simple) · **P2** navigation/selection · **P3** composite/portal · **P4** chart/layout/internal

Source list: `sources/COMPONENTS.md` (72 SCSS files from v9.7.0 harvest). Inventory waves 1–14: 74 YAML files.

| SCSS | P | Family | Inventory | Adapter / notes |
|------|---|--------|-----------|-----------------|
| _typography.scss | P0 | content | _typography.yaml | ContentTextPatterns + Playwright |
| _icons.scss | P0 | content | _icons.yaml | palette via theme + Playwright |
| _link.scss | P0 | content | _link.yaml | ContentTextPatterns + Playwright |
| _divider.scss | P0 | structure | _divider.yaml | StructurePatterns + Playwright |
| _paper.scss | P0 | structure | _paper.yaml | StructurePatterns + Playwright |
| _image.scss | P0 | content | _image.yaml | palette-only + Playwright |
| _flexbreak.scss | P4 | layout | _flexbreak.yaml | internal flex-wrap utility + Playwright |
| _grid.scss | P4 | layout | _grid.yaml | StructurePatterns |
| _layout.scss | P4 | layout | _layout.yaml | StructurePatterns mud-container |
| _form.scss | P4 | structure | _form.yaml | InputPatterns + Playwright |
| _field.scss | P2 | input | _field.yaml | InputPatterns + Playwright |
| _inputlabel.scss | P2 | input | _inputlabel.yaml | InputPatterns + Playwright |
| _inputcontrol.scss | P2 | input | _inputcontrol.yaml | InputPatterns + Playwright |
| _input.scss | P2 | input | _input.yaml | InputPatterns + Playwright |
| _focustrap.scss | P4 | a11y | _focustrap.yaml | focus utility |
| _overlay.scss | P2 | overlay | _overlay.yaml | StructurePatterns + Playwright |
| _button.scss | P1 | action | _button.yaml | Filled/Outlined + Playwright |
| _iconbutton.scss | P1 | action | _iconbutton.yaml | AccentTextPatterns + Playwright |
| _fab.scss | P1 | action | _fab.yaml | FilledPatterns + Playwright |
| _fabmenu.scss | P2 | action | _fabmenu.yaml | menu + fab composite + Playwright |
| _chip.scss | P1 | action | _chip.yaml | Filled/Outlined + Playwright |
| _buttongroup.scss | P1 | action | _buttongroup.yaml | grouped buttons + Playwright |
| _togglegroup.scss | P1 | toggle | _togglegroup.yaml | toggle group states + Playwright |
| _checkbox.scss | P1 | toggle | _checkbox.yaml | partial (icon-button) |
| _radio.scss | P1 | toggle | _radio.yaml | partial |
| _switch.scss | P1 | toggle | _switch.yaml | HR-132 state bridge |
| _slider.scss | P1 | input | _slider.yaml | channel thumb/track + Playwright |
| _rating.scss | P1 | input | _rating.yaml | star channel + Playwright |
| _tabs.scss | P1 | navigation | _tabs.yaml | HR-131 state bridge |
| _navmenu.scss | P2 | navigation | _navmenu.yaml | nav-link hover + Playwright |
| _breadcrumbs.scss | P2 | navigation | _breadcrumbs.yaml | AccentTextPatterns + Playwright |
| _list.scss | P2 | data | _list.yaml | list selection + Playwright |
| _treeview.scss | P2 | data | _treeview.yaml | expand/select + Playwright |
| _pagination.scss | P2 | navigation | _pagination.yaml | selected page + Playwright |
| _stepper.scss | P2 | navigation | _stepper.yaml | step states + Playwright |
| _toolbar.scss | P2 | layout | _toolbar.yaml | StructurePatterns + Playwright |
| _appbar.scss | P2 | layout | _appbar.yaml | StructurePatterns + Playwright |
| _card.scss | P2 | structure | _card.yaml | StructurePatterns + Playwright |
| _expansionpanel.scss | P2 | structure | _expansionpanel.yaml | expand/collapse + Playwright |
| _collapse.scss | P2 | structure | _collapse.yaml | expand state + Playwright |
| _dialog.scss | P2 | overlay | _dialog.yaml | StructurePatterns + Playwright |
| _popover.scss | P3 | overlay | _popover.yaml | portal surface rules + Playwright |
| _select.scss | P3 | picker | _select.yaml | HR-137 portal |
| _menu.scss | P2 | navigation | _menu.yaml | HR-137 portal + Playwright |
| _autocomplete.scss | P3 | picker | _autocomplete.yaml | HR-137 portal + Playwright |
| _picker.scss | P3 | picker | _picker.yaml | HR-137 portal suite + Playwright |
| _pickerdate.scss | P3 | picker | _pickerdate.yaml | HR-137 portal state |
| _pickertime.scss | P3 | picker | _pickertime.yaml | HR-137 portal + Playwright |
| _pickercolor.scss | P3 | picker | _pickercolor.yaml | HR-137 portal + Playwright |
| _table.scss | P2 | data | _table.yaml | StructurePatterns + Playwright |
| _simpletable.scss | P2 | data | _simpletable.yaml | StructurePatterns + Playwright |
| _datagrid.scss | P3 | data | _datagrid.yaml | composite table + Playwright |
| _alert.scss | P1 | feedback | _alert.yaml | Filled/Outlined patterns |
| _badge.scss | P1 | feedback | _badge.yaml | FilledPatterns |
| _snackbar.scss | P2 | feedback | _snackbar.yaml | FilledPatterns overlay toast + Playwright |
| _progresscircular.scss | P1 | feedback | _progresscircular.yaml | channel color + Playwright |
| _progresslinear.scss | P1 | feedback | _progresslinear.yaml | channel color + Playwright |
| _skeleton.scss | P0 | feedback | _skeleton.yaml | palette-only + Playwright |
| _tooltip.scss | P2 | overlay | _tooltip.yaml | StructurePatterns + Playwright |
| _carousel.scss | P3 | media | _carousel.yaml | slide selection + Playwright |
| _avatar.scss | P1 | content | _avatar.yaml | FilledPatterns + Playwright |
| _fileupload.scss | P2 | input | _fileupload.yaml | upload trigger + Playwright |
| _dropzone.scss | P2 | input | _dropzone.yaml | drag state + Playwright |
| _swipearea.scss | P4 | gesture | _swipearea.yaml | gesture utility |
| _splitpanel.scss | P4 | layout | _splitpanel.yaml | resize layout |
| _timeline.scss | P2 | data | _timeline.yaml | item states + Playwright |
| _charts.scss | P4 | chart | _chart.yaml | chart palette + Playwright |
| _consent.scss | P4 | internal | _consent.yaml | MudBlazor docs |
| _docspage.scss | P4 | internal | _docspage.yaml | MudBlazor docs |
| _docssection.scss | P4 | internal | _docssection.yaml | MudBlazor docs |
| _mudblazorapp.scss | P4 | internal | _mudblazorapp.yaml | MudBlazor shell |
| _widthslider.scss | P4 | internal | _widthslider.yaml | docs utility |
| _pagecontentnavigation.scss | P4 | internal | _pagecontentnavigation.yaml | docs nav |

**Summary:** 72/72 classified · 74 inventory YAML (wave 1–14) · wave 14: flexbreak, consent, docspage, docssection, mudblazorapp, widthslider, pagecontentnavigation + harvest CI gate

**Harvest:** `external/.../DesignTokens/scripts/harvest-mudblazor-inventory.sh`
