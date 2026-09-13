Name:           vgn
Version:        %{?pkg_version}%{!?pkg_version:0.0.0}
Release:        0
Summary:        Nuremberg transit departure monitor
License:        MIT
URL:            https://github.com/rlvelte/dotvgn
Source0:        %{name}-%{version}.tar.gz
ExclusiveArch:  x86_64
%define debug_package %{nil}

%description
Console monitor for VAG/VGN public transport departures,
built with DotVgn (https://github.com/rlvelte/dotvgn).

%prep
%autosetup -n %{name}-%{version}

%build

%install
install -Dm755 vgn %{buildroot}%{_bindir}/vgn

%check
:

%files
%license LICENSE
%{_bindir}/vgn

%changelog
* Sun Sep 13 2026 Robin L. Velte <rlvelte@users.noreply.github.com> - 2.1.1-0
- Show real station name when selecting a station by id

* Sun Sep 13 2026 Robin L. Velte <rlvelte@users.noreply.github.com> - 2.1.0-0
- Fully configurable CLI via command-line parameters (System.CommandLine)
- Add transport filter (-t), line filter (-l), limit (-n), timespan, and delay options
- Station argument accepts name or numeric station id
- Add --json flag for machine-readable output, including structured errors
- Route status output to stderr; handle API errors gracefully

* Sat Sep 12 2026 Robin L. Velte <rlvelte@users.noreply.github.com> - 2.0.0-0
- Release 2.0.0
