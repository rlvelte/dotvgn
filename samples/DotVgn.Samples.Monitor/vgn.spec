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
* Sat Sep 12 2026 Robin L. Velte <rlvelte@users.noreply.github.com> - 2.0.0-0
- Release 2.0.0
